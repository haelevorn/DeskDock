using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;
using Forms = System.Windows.Forms;

namespace DeskDock;

public partial class MainWindow : Window
{
    internal PlayerState Player { get; } = new();
    private readonly bool _testMode;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly SpotifyService _spotify = new();
    private readonly AudioService _audio = new();
    private TrayIcon? _tray;
    private System.Drawing.Icon? _trayImage;
    private readonly NotificationAudio _notificationAudio = new();
    private bool _refreshing, _sending, _syncingVolume, _closing, _webStarting;
    private string? _artKey;
    private int _notifications;
    private int? _unreadCount;
    private readonly NotificationDeduplicator _notificationGate = new();
    private string _webNotificationPermission = "sin consultar";
    private CoreWebView2Notification? _pendingNotification;
    private bool _layoutBusy;
    private string _layoutMode = "";
    private int _noticeTargetTab;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);

    private void WindowSourceInitialized(object? sender, EventArgs e)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            int roundedCorners = 2;
            _ = DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 33, ref roundedCorners, sizeof(int));
        }
    }

    private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseClick(object sender, RoutedEventArgs e) => Close();

    private void MoreClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { ContextMenu: { } menu } button)
        {
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }
    }

    private void StatusClick(object sender, RoutedEventArgs e) => MessageBox.Show(this,
        $"WhatsApp\n{WebStatus.Text}\n{LayoutStatus.Text}\n\nSpotify\n{MediaStatus.Text}\n\nAvisos\nPermiso del sitio: {_webNotificationPermission}\nEventos web recibidos: {_notifications}\nContador detectado: {_unreadCount?.ToString() ?? "sin contador"}\n{NotificationStatus.Text}\n\nAudio\n{AudioStatus.Text}",
        "Estado de DeskDock");

    public MainWindow() : this(false) { }

    internal MainWindow(bool testMode)
    {
        _testMode = testMode;
        InitializeComponent();
        DataContext = Player;
        var settings = Settings.Load();
        Pin.IsChecked = Topmost = settings.AlwaysOnTop;
        TrayOption.IsChecked = settings.MinimizeToTray;
        SoundOption.IsChecked = settings.NotificationSound;
        ApplySkin(settings.Skin);
        InitializeFocusTools(settings);
        RestoreRecentReferences(settings.RecentReferenceImages);
        _profiles = new(settings.Profiles);
        RefreshProfiles();
        if (!_testMode && !string.IsNullOrWhiteSpace(settings.ReferenceImagePath)) LoadReference(settings.ReferenceImagePath, save: false);
    }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
        if (_testMode) return;
        _focusTimer.Start();
        SetupTray();
        _timer.Tick += TimerTick;
        _timer.Start();
        await RefreshAsync();
        await StartWebAsync();
    }

    private void SetupTray()
    {
        try
        {
            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Mostrar DeskDock", null, (_, _) => Dispatcher.Invoke(Restore));
            menu.Items.Add("Salir", null, (_, _) => Dispatcher.Invoke(Close));
            using var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/DeskDock.ico"))!.Stream;
            using var icon = new System.Drawing.Icon(iconStream);
            _trayImage = (System.Drawing.Icon)icon.Clone();
            _tray = new TrayIcon
            {
                Text = "DeskDock", Icon = _trayImage, ContextMenuStrip = menu
            };
            _tray.Visible = true;
            _tray.DoubleClick += (_, _) => Dispatcher.Invoke(Restore);
            _tray.BalloonTipShown += (_, _) => Dispatcher.Invoke(() =>
            {
                ReportNotification(n => n.ReportShown());
                if (SoundOption.IsChecked) _notificationAudio.Play();
                NotificationStatus.Text = "Windows notificó que presentó el aviso. Comprueba si fue visible en tu pantalla.";
            });
            _tray.BalloonTipClicked += (_, _) => Dispatcher.Invoke(() =>
            {
                ReportNotification(n => n.ReportClicked());
                Restore();
                MainTabs.SelectedIndex = _noticeTargetTab;
            });
            _tray.BalloonTipClosed += (_, _) => Dispatcher.Invoke(ClosePendingNotification);
        }
        catch (Exception ex)
        {
            Diagnostics.Record("Tray", ex);
            _tray?.ContextMenuStrip?.Dispose();
            _tray?.Dispose();
            _tray = null;
            _trayImage?.Dispose();
            _trayImage = null;
            TrayOption.IsChecked = false;
            TrayOption.IsEnabled = false;
        }
    }

    private void Restore()
    {
        _ultraMini?.Hide();
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void HideClick(object sender, RoutedEventArgs e)
    {
        if (_tray is not null) Hide();
        else WindowState = WindowState.Minimized;
    }

    private void WindowStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized && TrayOption?.IsChecked == true && _tray is not null) Hide();
    }

    private void SettingsClick(object sender, RoutedEventArgs e)
    {
        Topmost = Pin.IsChecked == true;
        SaveSettings();
    }

    private void SaveSettings()
    {
        if (!_testMode)
        {
            var bounds = WindowState == WindowState.Normal ? new Rect(0, 0, Width, Height) : RestoreBounds;
            new Settings(Topmost, TrayOption.IsChecked, SkinManager.Current, SoundOption.IsChecked,
                SafeDimension(bounds.Width, 360, 300, SystemParameters.WorkArea.Width),
                SafeDimension(bounds.Height, 520, 340, SystemParameters.WorkArea.Height), Pomodoro.Options,
                _profiles.Items.ToList(), _referencePath, _recentReferencePaths.ToList()).Save();
        }
    }

    private void ApplySkin(string? id)
    {
        SkinManager.Apply(id);
        MinimalSkin.IsChecked = SkinManager.Current == "minimal";
        AuroraSkin.IsChecked = SkinManager.Current == "aurora";
        EclipseSkin.IsChecked = SkinManager.Current == "eclipse";
    }

    private void SkinClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.MenuItem { Tag: string id })
        {
            ApplySkin(id);
            SaveSettings();
        }
    }

    private async Task StartWebAsync()
    {
        if (_webStarting || _closing) return;
        _webStarting = true;
        try
        {
            if (Web.CoreWebView2 is not null)
            {
                Web.Reload();
                return;
            }
            string runtime = CoreWebView2Environment.GetAvailableBrowserVersionString();
            Directory.CreateDirectory(Diagnostics.Root);
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Diagnostics.Root, "WebView2"));
            if (_closing) return;
            await Web.EnsureCoreWebView2Async(environment);
            if (_closing) return;
            var core = Web.CoreWebView2 ?? throw new InvalidOperationException("WebView2 no está inicializado");
            core.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
            core.Settings.IsWebMessageEnabled = false;
            core.Settings.AreHostObjectsAllowed = false;
            core.PermissionRequested += WebPermission;
            core.NavigationStarting += (_, args) =>
            {
                if (NavigationPolicy.IsWhatsApp(args.Uri)) { _unreadCount = null; return; }
                args.Cancel = true;
                OpenExternal(args.Uri);
            };
            core.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                OpenExternal(args.Uri);
            };
            core.NavigationCompleted += (_, args) =>
            {
                if (_closing) return;
                WebStatus.Text = args.IsSuccess
                    ? "Página cargada. Comprueba el QR o tus chats; esto no verifica el inicio de sesión."
                    : $"No se pudo cargar WhatsApp ({args.WebErrorStatus}). Prueba Recargar o el navegador.";
            };
            core.NotificationReceived += (_, args) =>
            {
                if (!NavigationPolicy.IsWhatsApp(args.SenderOrigin)) return;
                // WPF sin registro de toasts: presentar un aviso propio mediante la bandeja.
                args.Handled = true;
                _notifications++;
                ShowNotice("Nuevo aviso de WhatsApp. Abre DeskDock para verlo.", args.Notification);
            };
            core.DocumentTitleChanged += (_, _) => CheckUnreadTitle();
            core.DOMContentLoaded += async (_, _) =>
            {
                await UpdateWebLayoutAsync();
                await ReadNotificationPermissionAsync();
            };
            core.ProcessFailed += (_, _) =>
            {
                if (!_closing) WebStatus.Text = "Un proceso de WebView2 falló. Cierra DeskDock y vuelve a abrirlo.";
            };
            environment.NewBrowserVersionAvailable += (_, _) => Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_closing) WebStatus.Text = "Hay una actualización de WebView2. Cierra y abre DeskDock para usarla.";
            }));
            WebStatus.Text = $"WebView2 {runtime}. Cargando WhatsApp…";
            core.Navigate("https://web.whatsapp.com/");
        }
        catch (WebView2RuntimeNotFoundException ex)
        {
            Diagnostics.Record("WebView2Runtime", ex);
            WebStatus.Text = "Falta Microsoft Edge WebView2 Runtime. Consulta INICIO-RAPIDO.md. Spotify puede seguir disponible.";
        }
        catch (Exception ex)
        {
            Diagnostics.Record("WebView2", ex);
            if (!_closing) WebStatus.Text = "No se pudo iniciar WebView2. Reinicia DeskDock o usa el navegador. Consulta diagnostics.log.";
        }
        finally { _webStarting = false; }
    }

    private void WebPermission(object? sender, CoreWebView2PermissionRequestedEventArgs e)
    {
        e.State = CoreWebView2PermissionState.Deny;
        if (!NavigationPolicy.IsWhatsApp(e.Uri) || e.PermissionKind != CoreWebView2PermissionKind.Notifications) return;
        var answer = MessageBox.Show(this, "¿Permitir que WhatsApp Web muestre notificaciones mientras DeskDock está abierto?", "DeskDock", MessageBoxButton.YesNo);
        e.State = answer == MessageBoxResult.Yes ? CoreWebView2PermissionState.Allow : CoreWebView2PermissionState.Deny;
        _webNotificationPermission = answer == MessageBoxResult.Yes ? "granted" : "denied";
        WebNoticeState.Text = answer == MessageBoxResult.Yes ? "Avisos ✓" : "Activar avisos";
        NotificationStatus.Text = answer == MessageBoxResult.Yes
            ? "Permiso concedido. Falta comprobar la recepción real de un mensaje."
            : "Notificaciones denegadas. Puedes solicitarlas de nuevo desde WhatsApp.";
    }

    private async void ReloadClick(object sender, RoutedEventArgs e) => await StartWebAsync();
    private void BrowserClick(object sender, RoutedEventArgs e) => OpenExternal("https://web.whatsapp.com/");
    private void SpotifyClick(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("spotify:") { UseShellExecute = true }); }
        catch (Exception ex)
        {
            Diagnostics.Record("SpotifyLaunch", ex);
            MediaStatus.Text = "Windows no pudo abrir spotify:. Abre Spotify manualmente desde Inicio.";
        }
    }

    private void OpenExternal(string url)
    {
        if (!NavigationPolicy.IsExternalWeb(url))
        {
            WebStatus.Text = "Se bloqueó un enlace que no es HTTP/HTTPS. Abre WhatsApp en tu navegador si lo necesitas.";
            return;
        }
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex)
        {
            Diagnostics.Record("Browser", ex);
            WebStatus.Text = "No se pudo abrir el navegador predeterminado.";
        }
    }

    private void DiagnosticsClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Diagnostics.Root);
            Process.Start(new ProcessStartInfo("explorer.exe", Diagnostics.Root) { UseShellExecute = true });
        }
        catch (Exception ex) { Diagnostics.Record("Explorer", ex); }
    }

    private async void TimerTick(object? sender, EventArgs e)
    {
        await RefreshAsync();
        await UpdateWebLayoutAsync();
    }

    private async void EnableNotificationsClick(object sender, RoutedEventArgs e)
    {
        if (Web.CoreWebView2 is not { } core) { WebStatus.Text = "Espera a que WhatsApp termine de cargar."; return; }
        try
        {
            await core.Profile.SetPermissionStateAsync(CoreWebView2PermissionKind.Notifications,
                "https://web.whatsapp.com", CoreWebView2PermissionState.Allow);
            _webNotificationPermission = "granted";
            WebNoticeState.Text = "Avisos ✓";
            NotificationStatus.Text = "Permiso de avisos activado. Activa también las notificaciones en los ajustes de WhatsApp y recibe un mensaje de prueba.";
            WebStatus.Text = "Permiso activado. Revisa Ajustes → Notificaciones dentro de WhatsApp. Usa Probar aviso para comprobar Windows.";
        }
        catch (Exception ex) { Diagnostics.Record("NotificationPermission", ex); WebStatus.Text = "No se pudo activar el permiso. Prueba recargar WhatsApp."; }
    }

    private void TestNotificationClick(object sender, RoutedEventArgs e) =>
        ShowNotice("Este es un aviso de prueba de DeskDock. No procede de un mensaje de WhatsApp.", test: true);

    private async Task ReadNotificationPermissionAsync()
    {
        if (_closing || Web.CoreWebView2 is not { } core || !NavigationPolicy.IsWhatsApp(core.Source)) return;
        try
        {
            string json = await core.ExecuteScriptAsync("typeof Notification !== 'undefined' ? Notification.permission : 'unsupported'");
            if (_closing) return;
            _webNotificationPermission = JsonSerializer.Deserialize<string>(json) ?? "desconocido";
            WebNoticeState.Text = _webNotificationPermission == "granted" ? "Avisos ✓" : "Activar avisos";
            WebNoticeState.ToolTip = _webNotificationPermission == "granted"
                ? "Permiso web concedido. Revisa también notificaciones dentro de WhatsApp."
                : "Pulsa para permitir avisos de WhatsApp en este perfil.";
        }
        catch (Exception ex) { Diagnostics.Record("NotificationPermissionRead", ex); }
    }

    private void CheckUnreadTitle()
    {
        if (_closing || Web.CoreWebView2 is not { } core || !NavigationPolicy.IsWhatsApp(core.Source)) return;
        int? next = NotificationPolicy.UnreadCount(core.DocumentTitle);
        if (!next.HasValue) return;
        int? previous = _unreadCount;
        _unreadCount = next;
        // No anunciar el contador inicial al cargar el perfil.
        if (previous.HasValue && next > previous && (!IsVisible || !IsActive || MainTabs.SelectedIndex != 0))
            ShowNotice("Ha aumentado el contador de mensajes sin leer de WhatsApp. Abre DeskDock para revisarlo.", source: WhatsAppNoticeSource.UnreadCounter);
    }

    private void ShowNotice(string text, CoreWebView2Notification? notification = null, bool test = false, string? title = null, int targetTab = 0,
        WhatsAppNoticeSource source = WhatsAppNoticeSource.WebEvent)
    {
        if (_closing) return;
        // Evitar duplicar un aviso web y el cambio de título del mismo mensaje.
        if (targetTab == 0 && !test && !_notificationGate.ShouldPresent(source, TimeSpan.FromMilliseconds(Environment.TickCount64)))
        {
            try { notification?.ReportClosed(); } catch (Exception ex) { Diagnostics.Record("NotificationClose", ex); }
            return;
        }
        if (_tray is null)
        {
            NotificationStatus.Text = "No está disponible el icono de bandeja; no se pueden presentar avisos.";
            return;
        }
        ClosePendingNotification();
        _pendingNotification = notification;
        _noticeTargetTab = targetTab;
        try
        {
            _tray.ShowBalloonTip(6000, title ?? (test ? "DeskDock · prueba de aviso" : "WhatsApp · DeskDock"), text, Forms.ToolTipIcon.Info);
            NotificationStatus.Text = $"Aviso enviado a Windows. Eventos web recibidos: {_notifications}. Si no aparece, revisa notificaciones y No molestar en Windows.";
        }
        catch (Exception ex) { Diagnostics.Record("NotificationShow", ex); ClosePendingNotification(); }
    }

    private void ReportNotification(Action<CoreWebView2Notification> action)
    {
        try { if (_pendingNotification is not null) action(_pendingNotification); }
        catch (Exception ex) { Diagnostics.Record("NotificationLifecycle", ex); }
    }

    private void ClosePendingNotification()
    {
        ReportNotification(n => n.ReportClosed());
        _pendingNotification = null;
    }

    private async void WebSizeChanged(object sender, SizeChangedEventArgs e) => await UpdateWebLayoutAsync();
    private async void CompactClick(object sender, RoutedEventArgs e) => await UpdateWebLayoutAsync();
    private async void ChatListClick(object sender, RoutedEventArgs e) { MainTabs.SelectedIndex = 0; _layoutMode = "list"; await UpdateWebLayoutAsync(); }
    private async void ConversationClick(object sender, RoutedEventArgs e) { MainTabs.SelectedIndex = 0; _layoutMode = "auto"; await UpdateWebLayoutAsync(); }

    private async Task UpdateWebLayoutAsync()
    {
        if (_closing || _layoutBusy || Web?.CoreWebView2 is not { } core || !NavigationPolicy.IsWhatsApp(core.Source) || Web.ActualWidth < 1) return;
        _layoutBusy = true;
        try
        {
            await core.ExecuteScriptAsync(WhatsAppLayout.InstallScript);
            bool enabled = CompactWhatsApp.IsChecked == true;
            string width = Web.ActualWidth.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string result = await core.ExecuteScriptAsync($"window.__deskDockLayout.configure({(enabled ? "true" : "false")}, '{_layoutMode}', {width})");
            _layoutMode = "";
            if (_closing) return;
            using var document = JsonDocument.Parse(result);
            string? state = document.RootElement.GetProperty("state").GetString();
            if (enabled && state == "unsupported")
            {
                // QR, carga o estructura desconocida: mostrar toda la web reducida como alternativa.
                Web.ZoomFactor = System.Math.Clamp(Web.ActualWidth / 1100.0, 0.25, 1);
                LayoutStatus.Text = "Ajuste al ancho. Al abrir una conversación se intentará la vista de chat completo.";
            }
            else
            {
                if (Web.ZoomFactor != 1) Web.ZoomFactor = 1;
                LayoutStatus.Text = state switch
                {
                    "chat" => "Conversación a todo el ancho · Chats vuelve a la lista.",
                    "list" => "Lista a todo el ancho · Selecciona una conversación y pulsa Chat si no cambia sola.",
                    "wide" => "Vista amplia de WhatsApp.",
                    _ => "Vista original. Activa Vista adaptable para ventanas pequeñas."
                };
            }
        }
        catch (Exception ex) { Diagnostics.Record("WebLayout", ex); if (!_closing) LayoutStatus.Text = "No se pudo adaptar esta vista. Puedes desactivar Vista adaptable."; }
        finally { _layoutBusy = false; }
    }

    private async Task RefreshAsync()
    {
        if (_refreshing || _closing) return;
        _refreshing = true;
        try
        {
            RefreshAudio();
            var session = await _spotify.FindAsync();
            if (_closing) return;
            if (session is null)
            {
                ClearMedia();
                MediaStatus.Text = "Windows no expone una sesión de Spotify reconocida. Ábrelo y reproduce una canción. Otras aplicaciones no se controlarán.";
                return;
            }
            var properties = await session.TryGetMediaPropertiesAsync();
            if (_closing) return;
            var info = session.GetPlaybackInfo();
            Player.IsPlaying = info.PlaybackStatus == Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            Player.Title = string.IsNullOrWhiteSpace(properties.Title) ? "Spotify no publica título" : properties.Title;
            Player.Artist = properties.Artist;
            Player.CanPlay = !_sending && info.Controls.IsPlayEnabled;
            Player.CanPause = !_sending && info.Controls.IsPauseEnabled;
            Player.CanPrevious = !_sending && info.Controls.IsPreviousEnabled;
            Player.CanNext = !_sending && info.Controls.IsNextEnabled;
            MediaStatus.Text = $"Sesión detectada: {session.SourceAppUserModelId}\nEstado publicado por Windows: {info.PlaybackStatus}.";
            var key = session.SourceAppUserModelId + "\n" + properties.Title + "\n" + properties.Artist + "\n" + properties.AlbumTitle;
            if (key != _artKey)
            {
                Player.Artwork = null;
                _artKey = null;
                if (properties.Thumbnail is not null)
                {
                    try
                    {
                        using var stream = await properties.Thumbnail.OpenReadAsync();
                        // Límite para evitar imágenes inesperadamente grandes de una sesión externa.
                        if (stream.Size > 0 && stream.Size <= 8_000_000)
                        {
                            using var reader = new DataReader(stream.GetInputStreamAt(0));
                            uint size = (uint)stream.Size;
                            uint loaded = await reader.LoadAsync(size);
                            if (loaded == size)
                            {
                                var bytes = new byte[(int)size];
                                reader.ReadBytes(bytes);
                                using var memory = new MemoryStream(bytes);
                                var bitmap = new BitmapImage();
                                bitmap.BeginInit();
                                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap.DecodePixelWidth = 512;
                                bitmap.StreamSource = memory;
                                bitmap.EndInit();
                                bitmap.Freeze();
                                if (!_closing) Player.Artwork = bitmap;
                            }
                        }
                    }
                    catch (Exception ex) { Diagnostics.Record("Artwork", ex); }
                }
                _artKey = key;
            }
        }
        catch (Exception ex)
        {
            Diagnostics.Record("MediaRead", ex);
            if (!_closing)
            {
                ClearMedia();
                MediaStatus.Text = "No se pudo consultar la sesión multimedia. Windows puede negar el acceso o Spotify puede haberse cerrado. Se reintentará.";
            }
        }
        finally { _refreshing = false; }
    }

    private void ClearMedia()
    {
        Player.IsPlaying = false;
        Player.CanPlay = Player.CanPause = Player.CanPrevious = Player.CanNext = false;
        Player.Title = "Sin sesión de Spotify";
        Player.Artist = "Abre Spotify y reproduce música";
        Player.Artwork = null;
        _artKey = null;
    }

    private async Task SendAsync(MediaCommand command)
    {
        if (_sending || _closing) return;
        _sending = true;
        Player.CanPlay = Player.CanPause = Player.CanPrevious = Player.CanNext = false;
        try
        {
            bool accepted = await _spotify.SendAsync(command);
            if (!_closing && !accepted)
                MessageBox.Show(this, "Windows o Spotify no aceptó el comando. Comprueba la reproducción en Spotify.", "DeskDock");
        }
        catch (Exception ex)
        {
            Diagnostics.Record("MediaCommand", ex);
            if (!_closing) MessageBox.Show(this, "No se pudo enviar el comando a Spotify.", "DeskDock");
        }
        finally { _sending = false; }
        await RefreshAsync();
    }

    private async void PlayClick(object sender, RoutedEventArgs e) => await SendAsync(MediaCommand.Play);
    private async void PauseClick(object sender, RoutedEventArgs e) => await SendAsync(MediaCommand.Pause);
    private async void PreviousClick(object sender, RoutedEventArgs e) => await SendAsync(MediaCommand.Previous);
    private async void NextClick(object sender, RoutedEventArgs e) => await SendAsync(MediaCommand.Next);

    private void RefreshAudio()
    {
        try
        {
            var state = _audio.Read();
            _syncingVolume = true;
            if (!MasterVolume.IsMouseCaptureWithin && !MasterVolume.IsKeyboardFocusWithin) MasterVolume.Value = state.Volume * 100;
            VolumeValue.Text = $"{state.Volume * 100:0}%";
            Mute.Content = state.Muted ? "Activar sonido" : "Silenciar";
            MasterVolume.IsEnabled = Mute.IsEnabled = true;
            AudioStatus.Text = "Volumen maestro · " + state.Device;
        }
        catch (Exception ex)
        {
            Diagnostics.Record("AudioRead", ex);
            MasterVolume.IsEnabled = Mute.IsEnabled = false;
            VolumeValue.Text = "—";
            AudioStatus.Text = "Sin acceso al dispositivo de audio. Se reintentará.";
        }
        finally { _syncingVolume = false; }
    }

    private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_syncingVolume || !IsLoaded || _closing) return;
        try { _audio.SetVolume((float)(e.NewValue / 100)); VolumeValue.Text = $"{e.NewValue:0}%"; }
        catch (Exception ex) { Diagnostics.Record("AudioVolume", ex); RefreshAudio(); }
    }

    private void MuteClick(object sender, RoutedEventArgs e)
    {
        try { _audio.ToggleMute(); }
        catch (Exception ex) { Diagnostics.Record("AudioMute", ex); }
        RefreshAudio();
    }

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        _closing = true;
        _ultraMini?.CloseForExit();
        CloseChatGpt();
        _focusTimer.Stop();
        _focusTimer.Tick -= FocusTimerTick;
        if (_testMode) { Web.Dispose(); return; }
        _timer.Stop();
        _timer.Tick -= TimerTick;
        SaveSettings();
        if (_tray is not null)
        {
            ClosePendingNotification();
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
        }
        _trayImage?.Dispose();
        _notificationAudio.Dispose();
        Web.Dispose();
        Application.Current.Shutdown();
    }
}
