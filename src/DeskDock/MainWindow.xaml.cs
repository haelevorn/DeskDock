using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Windows.Storage.Streams;
using Forms = System.Windows.Forms;

namespace DeskDock;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private readonly SpotifyService _spotify = new();
    private readonly AudioService _audio = new();
    private Forms.NotifyIcon? _tray;
    private bool _refreshing, _sending, _syncingVolume, _closing, _webStarting;
    private string? _artKey;
    private int _notifications;

    public MainWindow()
    {
        InitializeComponent();
        var settings = Settings.Load();
        Pin.IsChecked = Topmost = settings.AlwaysOnTop;
        TrayOption.IsChecked = settings.MinimizeToTray;
    }

    private async void WindowLoaded(object sender, RoutedEventArgs e)
    {
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
            _tray = new Forms.NotifyIcon
            {
                Text = "DeskDock", Icon = System.Drawing.SystemIcons.Application,
                ContextMenuStrip = menu, Visible = true
            };
            _tray.DoubleClick += (_, _) => Dispatcher.Invoke(Restore);
        }
        catch (Exception ex)
        {
            Diagnostics.Record("Tray", ex);
            TrayOption.IsChecked = false;
            TrayOption.IsEnabled = false;
        }
    }

    private void Restore()
    {
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
        new Settings(Topmost, TrayOption.IsChecked == true).Save();
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
            var core = Web.CoreWebView2;
            core.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
            core.Settings.IsWebMessageEnabled = false;
            core.Settings.AreHostObjectsAllowed = false;
            core.PermissionRequested += WebPermission;
            core.NavigationStarting += (_, args) =>
            {
                if (NavigationPolicy.IsWhatsApp(args.Uri)) return;
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
                // Conservar la UI predeterminada de WebView2. No leer el contenido privado.
                args.Handled = false;
                _notifications++;
                NotificationStatus.Text = $"WebView2 ha recibido {_notifications} notificación(es). Comprueba si Windows muestra el aviso.";
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

    private async void TimerTick(object? sender, EventArgs e) => await RefreshAsync();

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
            TrackTitle.Text = string.IsNullOrWhiteSpace(properties.Title) ? "Spotify no publica título" : properties.Title;
            TrackArtist.Text = properties.Artist;
            Play.IsEnabled = !_sending && info.Controls.IsPlayEnabled;
            Pause.IsEnabled = !_sending && info.Controls.IsPauseEnabled;
            Previous.IsEnabled = !_sending && info.Controls.IsPreviousEnabled;
            Next.IsEnabled = !_sending && info.Controls.IsNextEnabled;
            MediaStatus.Text = $"Sesión detectada: {session.SourceAppUserModelId}\nEstado publicado por Windows: {info.PlaybackStatus}.";
            var key = session.SourceAppUserModelId + "\n" + properties.Title + "\n" + properties.Artist + "\n" + properties.AlbumTitle;
            if (key != _artKey)
            {
                Artwork.Source = null;
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
                                bitmap.DecodePixelWidth = 128;
                                bitmap.StreamSource = memory;
                                bitmap.EndInit();
                                bitmap.Freeze();
                                if (!_closing) Artwork.Source = bitmap;
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
        Play.IsEnabled = Pause.IsEnabled = Previous.IsEnabled = Next.IsEnabled = false;
        TrackTitle.Text = "Sin sesión de Spotify";
        TrackArtist.Text = "Abre Spotify y reproduce música";
        Artwork.Source = null;
        _artKey = null;
    }

    private async Task SendAsync(MediaCommand command)
    {
        if (_sending || _closing) return;
        _sending = true;
        Play.IsEnabled = Pause.IsEnabled = Previous.IsEnabled = Next.IsEnabled = false;
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
        _timer.Stop();
        _timer.Tick -= TimerTick;
        new Settings(Topmost, TrayOption.IsChecked == true).Save();
        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
        }
        Web.Dispose();
        Application.Current.Shutdown();
    }
}
