using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeskDock;

// Pruebas sin cuentas, navegador, altavoces o Spotify. Las integraciones requieren prueba manual.
internal static class SelfTests
{
    public static int Run()
    {
        var results = new List<string>();
        try
        {
            FocusSelfTests.Run(results);
            Check(SpotifyService.IsSpotifyId("Spotify.exe"), "Spotify Win32", results);
            Check(SpotifyService.IsSpotifyId("SpotifyAB.SpotifyMusic_zpdnekdrzrea0!Spotify"), "Spotify Store", results);
            foreach (string id in new[] { "msedge.exe", "chrome.exe", "OtherSpotify.exe", "", "SpotifyAB.Fake!Spotify" })
                Check(!SpotifyService.IsSpotifyId(id), "Rechazar otra sesión: " + id, results);
            Check(NavigationPolicy.IsWhatsApp("https://web.whatsapp.com/"), "Origen WhatsApp", results);
            Check(NavigationPolicy.IsWhatsApp("https://web.whatsapp.com/path?x=1"), "Ruta WhatsApp", results);
            foreach (string uri in new[] { "http://web.whatsapp.com", "https://web.whatsapp.com.attacker.test", "https://web.whatsapp.com:444/", "https://user@web.whatsapp.com", "javascript:alert(1)", "file:///C:/Windows/", "invalid" })
                Check(!NavigationPolicy.IsWhatsApp(uri), "Rechazar origen: " + uri, results);
            Check(NavigationPolicy.IsExternalWeb("https://example.com/"), "Enlace HTTPS", results);
            foreach (string uri in new[] { "file:///C:/Windows", "javascript:alert(1)", "spotify:", "ms-settings:", "https://user:pass@example.com" })
                Check(!NavigationPolicy.IsExternalWeb(uri), "Rechazar protocolo/credencial: " + uri, results);
            var settings = new Settings(true, false, "eclipse", false);
            Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings)) == settings, "Persistencia de opciones", results);
            var oldSettings = JsonSerializer.Deserialize<Settings>("{\"AlwaysOnTop\":true,\"MinimizeToTray\":false}")!;
            Check(oldSettings.Skin == "minimal" && oldSettings.NotificationSound, "Migración de preferencias anteriores sin perder opciones", results);
            Check(SkinManager.Normalize("../../otro") == "minimal", "Skins limitados a recursos incluidos", results);
            Check(TrayIcon.NativeDataSize == 976 && (TrayIcon.SilentCustomBalloon & 0x10) != 0,
                "Estructura Shell x64 y aviso con sonido de Windows desactivado", results);
            using (var sound = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/notification-bwoop.wav"))!.Stream)
            using (var reader = new BinaryReader(sound))
            {
                Check(new string(reader.ReadChars(4)) == "RIFF" && sound.Length > 40000, "WAV original incluido en el ejecutable", results);
                sound.Position = 22;
                Check(reader.ReadUInt16() == 1 && reader.ReadInt32() == 44100, "Sonido PCM mono a 44.1 kHz", results);
            }
            Check(NotificationPolicy.UnreadCount("(3) WhatsApp") == 3, "Contador de no leídos", results);
            Check(NotificationPolicy.UnreadCount("WhatsApp") == 0, "Contador reiniciado", results);
            Check(NotificationPolicy.UnreadCount("WhatsApp (4)") == 4 && NotificationPolicy.UnreadCount("\u200e(2) WhatsApp Web\u200f") == 2,
                "Contador admite sufijo, formato web y marcas de dirección", results);
            var gate = new NotificationDeduplicator();
            Check(gate.ShouldPresent(WhatsAppNoticeSource.WebEvent, TimeSpan.Zero) &&
                  gate.ShouldPresent(WhatsAppNoticeSource.WebEvent, TimeSpan.FromMilliseconds(400)),
                "Dos eventos web consecutivos no se pierden por límite temporal", results);
            Check(!gate.ShouldPresent(WhatsAppNoticeSource.UnreadCounter, TimeSpan.FromMilliseconds(600)) &&
                   gate.ShouldPresent(WhatsAppNoticeSource.WebEvent, TimeSpan.FromMilliseconds(900)),
                "Agrupar solo duplicado contador-evento sin bloquear el siguiente aviso", results);
            foreach (string title in new[] { "(3) Otro sitio", "WhatsApp - contacto", "(texto) WhatsApp", "(999999999999999) WhatsApp" })
                Check(NotificationPolicy.UnreadCount(title) is null, "Ignorar título no reconocido", results);

            // Carga el XAML real con sus controles sin iniciar servicios ni mostrar la ventana.
            var window = new MainWindow(testMode: true) { ShowInTaskbar = false, Opacity = 0 };
            window.Show();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(WindowCorners.HasClippedCorner(window), "Recorte nativo excluye las esquinas de la ventana completa", results);
            Check(((System.Windows.FrameworkElement)window.FindName("MiniPlayer")).ActualHeight <= 60,
                "Mini reproductor compacto sin filas extra", results);
            Check(((System.Windows.FrameworkElement)window.FindName("Web")).ActualHeight > window.ActualHeight * 0.62,
                "WhatsApp conserva espacio con barra compacta de navegación", results);
            Check(((System.Windows.Controls.Button)window.FindName("ChatListButton")).IsVisible &&
                  ((System.Windows.Controls.Button)window.FindName("ConversationButton")).IsVisible &&
                  ((FrameworkElement)window.FindName("WhatsAppToolbar")).ActualHeight <= 28,
                "Chats y Chat visibles sin abrir menús", results);
            Check(window.MinWidth == 300 && window.MinHeight == 340, "Ventana mínima de 300 por 340", results);
            Check(((System.Windows.Controls.TextBlock)window.FindName("ClockText")).Text.Length == 5,
                "Reloj de Windows visible desde el inicio", results);
            Check(((System.Windows.Controls.TabControl)window.FindName("MainTabs")).Items.Count == 5,
                "Cinco pestañas incluida ChatGPT", results);
            foreach (string url in new[] { "https://chatgpt.com/", "https://auth.openai.com/authorize", "https://accounts.google.com/" })
                Check(NavigationPolicy.IsChatGptNavigation(url), "Dominios previstos para acceso web de ChatGPT", results);
            foreach (string url in new[] { "https://chatgpt.com.attacker.test/", "http://chatgpt.com/", "https://user@chatgpt.com/", "file:///C:/Windows/", "https://openai.com.attacker.test/" })
                Check(!NavigationPolicy.IsChatGptNavigation(url), "Rechazar origen externo en pestaña ChatGPT", results);
            Check(((FrameworkElement)window.FindName("PomoBadge")).Visibility == Visibility.Collapsed,
                "Indicador Pomodoro oculto al estar inactivo", results);
            window.Pomodoro.Start();
            window.RenderPomodoro();
            Check(((FrameworkElement)window.FindName("PomoBadge")).Visibility == Visibility.Visible,
                "Indicador Pomodoro visible aunque esté en WhatsApp", results);
            window.Pomodoro.Pause();
            window.RenderPomodoro();
            Check(((FrameworkElement)window.FindName("PomoBadge")).Visibility == Visibility.Collapsed,
                "Pausar retira el indicador activo", results);
            window.Pomodoro.Reset();
            window.RenderPomodoro();
            Check(window.FindName("Web") is Microsoft.Web.WebView2.Wpf.WebView2, "WebView2 WPF en XAML", results);
            Check(window.FindName("MasterVolume") is System.Windows.Controls.Slider, "Control de volumen en XAML", results);
            Check(window.FindName("Play") is System.Windows.Controls.Button, "Control de reproducción en XAML", results);
            Check(window.WindowStyle == WindowStyle.None, "Sin barra de título estándar", results);
            Check(window.Icon is not null, "Icono propio de DeskDock cargado en WPF", results);
            Check(System.Windows.Shell.WindowChrome.GetWindowChrome(window) is not null, "Arrastre y bordes con WindowChrome", results);
            // Las dos vistas deben compartir datos y disponibilidad, sin otra consulta a Spotify.
            ((System.Windows.Controls.TabControl)window.FindName("MainTabs")).SelectedIndex = 1;
            window.Measure(new Size(490, 740));
            window.Arrange(new Rect(0, 0, 490, 740));
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            var titleSource = (System.Windows.Controls.TextBlock)window.FindName("TrackTitle");
            var titleTarget = (System.Windows.Controls.TextBlock)window.FindName("FullTrackTitle");
            window.Player.Title = "Canción de prueba";
            System.Windows.Data.BindingOperations.GetBindingExpression(titleSource, System.Windows.Controls.TextBlock.TextProperty)!.UpdateTarget();
            System.Windows.Data.BindingOperations.GetBindingExpression(titleTarget, System.Windows.Controls.TextBlock.TextProperty)!.UpdateTarget();
            Check(titleTarget.Text == "Canción de prueba" && titleSource.Text == titleTarget.Text,
                $"Título sincronizado en Spotify y mini reproductor (mini={titleSource.Text}; pestaña={titleTarget.Text})", results);
            var fullPlay = (System.Windows.Controls.Button)window.FindName("FullPlay");
            var miniPlay = (System.Windows.Controls.Button)window.FindName("Play");
            window.Player.CanPlay = true;
            System.Windows.Data.BindingOperations.GetBindingExpression(miniPlay, System.Windows.Controls.Button.IsEnabledProperty)!.UpdateTarget();
            System.Windows.Data.BindingOperations.GetBindingExpression(fullPlay, System.Windows.Controls.Button.IsEnabledProperty)!.UpdateTarget();
            Check(fullPlay.IsEnabled, "Disponibilidad sincronizada de reproducir", results);
            window.Player.IsPlaying = true;
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(miniPlay.Visibility == Visibility.Collapsed && fullPlay.Visibility == Visibility.Collapsed,
                "Ocultar reproducir mientras Spotify está reproduciendo", results);
            Check(((System.Windows.Controls.Button)window.FindName("Pause")).Visibility == Visibility.Visible &&
                  ((System.Windows.Controls.Button)window.FindName("FullPause")).Visibility == Visibility.Visible,
                "Mostrar pausa en el mismo espacio en ambas vistas", results);
            window.Width = 300;
            window.Height = 340;
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(((FrameworkElement)window.FindName("SpotifyCover")).ActualHeight <= 60,
                "Portada reducida al tamaño mínimo de ventana", results);
            Check(((FrameworkElement)window.FindName("MiniArtworkFrame")).Visibility == Visibility.Collapsed,
                "Mini reproductor libera espacio al estrechar ventana", results);
            var tabs = (System.Windows.Controls.TabControl)window.FindName("MainTabs");
            tabs.SelectedIndex = 2;
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            var start = (System.Windows.Controls.Button)window.FindName("PomoStart");
            Check(start.ActualWidth > 0 && ((System.Windows.Controls.TextBlock)window.FindName("PomoTime")).Text == "25:00",
                "Contador y botón Pomodoro presentes en ventana pequeña", results);
            ((System.Windows.Controls.Expander)window.FindName("PomoConfigExpander")).IsExpanded = true;
            ((System.Windows.Controls.Expander)window.FindName("ProfilesExpander")).IsExpanded = true;
            window.UpdateLayout();
            Check(((System.Windows.Controls.ComboBox)window.FindName("ProfilePicker")).Template is not null,
                "Controles de perfiles y configuración cargan con el skin", results);
            tabs.SelectedIndex = 3;
            window.UpdateLayout();
            string imagePath = Path.Combine(Path.GetTempPath(), "DeskDock-reference-" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                var sample = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgra32, null,
                    new byte[] { 0, 0, 255, 255, 0, 255, 0, 255, 255, 0, 0, 255, 255, 255, 255, 255 }, 8);
                var imageEncoder = new PngBitmapEncoder();
                imageEncoder.Frames.Add(BitmapFrame.Create(sample));
                using (var imageFile = File.Create(imagePath)) imageEncoder.Save(imageFile);
                Check(window.LoadReference(imagePath, save: false), "Abrir imagen local de referencia", results);
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                var photo = (System.Windows.Controls.Image)window.FindName("ReferencePhoto");
                var canvas = (FrameworkElement)window.FindName("ReferenceCanvas");
                var bounds = photo.TransformToAncestor(canvas).TransformBounds(new Rect(0, 0, photo.ActualWidth, photo.ActualHeight));
                Check(bounds.Width > 0 && bounds.Right <= canvas.ActualWidth + 1 && bounds.Bottom <= canvas.ActualHeight + 1,
                    "Referencia se ajusta completa dentro del panel pequeño", results);
                double beforeZoom = ((MatrixTransform)photo.RenderTransform).Matrix.M11;
                var zoomIn = (System.Windows.Controls.Button)window.FindName("ReferenceZoomInButton");
                var zoomOut = (System.Windows.Controls.Button)window.FindName("ReferenceZoomOutButton");
                zoomIn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Check(((MatrixTransform)photo.RenderTransform).Matrix.M11 > beforeZoom, "Lupa más acerca incluso imágenes pequeñas", results);
                zoomOut.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Check(Math.Abs(((MatrixTransform)photo.RenderTransform).Matrix.M11 - beforeZoom) < 0.001,
                    "Lupa menos reduce el zoom con la acción correcta", results);
                Check(zoomIn.Content is System.Windows.Shapes.Path && zoomOut.Content is System.Windows.Shapes.Path,
                    "Iconos de referencia vectoriales sin depender de glifos de fuente", results);
                ((System.Windows.Controls.Button)window.FindName("ReferenceRotateButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                double rotatedScale = ReferenceTransforms.Scale(((MatrixTransform)photo.RenderTransform).Matrix);
                zoomIn.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Check(ReferenceTransforms.Scale(((MatrixTransform)photo.RenderTransform).Matrix) > rotatedScale,
                    "Zoom mantiene el giro de referencia", results);
                ((System.Windows.Controls.Button)window.FindName("ReferenceFlipButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                Check(((MatrixTransform)photo.RenderTransform).Matrix.Determinant < 0, "Volteo horizontal invierte la referencia", results);
                var recentButton = (System.Windows.Controls.Button)window.FindName("ReferenceRecentButton");
                recentButton.ContextMenu!.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.ContextMenu.OpenedEvent));
                Check(recentButton.ContextMenu.Items.Count >= 1, "Imagen abierta disponible en menú Recientes", results);
                ((System.Windows.Controls.Button)window.FindName("ReferenceFitButton")).RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                File.Delete(imagePath);
                Check(((BitmapSource)photo.Source).PixelWidth == 2, "Imagen queda en memoria sin bloquear el archivo original", results);
            }
            finally { if (File.Exists(imagePath)) File.Delete(imagePath); }
            Check(window.AllowsTransparency == false && window.FindName("GlassOption") is null,
                "Ventana opaca sin opción Glass", results);
            var mini = new UltraMiniWindow(window.Player, () => { }, () => { }, _ => { }) { ShowInTaskbar = false, Opacity = 0 };
            mini.UpdateStatus("12:34", null);
            mini.Show();
            mini.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(ReferenceEquals(mini.DataContext, window.Player) && mini.ActualHeight <= 100,
                "Ultramini comparte reproductor y ocupa menos de 100 de alto", results);
            Check(WindowCorners.HasClippedCorner(mini), "Ultramini aplica el mismo recorte que su borde redondeado", results);
            mini.UpdateStatus("12:34", "04:59 · Concentración");
            Check(((FrameworkElement)mini.FindName("MiniPomodoro")).Visibility == Visibility.Visible,
                "Pomodoro activo visible en ultramini", results);
            mini.UpdateStatus("12:34", null);
            Check(((FrameworkElement)mini.FindName("MiniPomodoro")).Visibility == Visibility.Collapsed,
                "Ultramini oculta Pomodoro inactivo", results);
            mini.CloseForExit();
            tabs.SelectedIndex = 1;
            window.Width = 360;
            window.Height = 520;
            window.UpdateLayout();
            foreach (string skin in new[] { "aurora", "eclipse", "minimal" })
            {
                SkinManager.Apply(skin);
                window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                Check(ReferenceEquals(window.Background, Application.Current.Resources["WindowBackground"]),
                    "Skin " + skin + " actualiza el fondo sin reiniciar", results);
                Check(((System.Windows.FrameworkElement)window.FindName("MiniPlayer")).ActualHeight <= 60,
                    "Skin " + skin + " conserva el tamaño compacto", results);
                var root = (FrameworkElement)window.Content;
                var preview = new RenderTargetBitmap((int)window.ActualWidth, (int)window.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                preview.Render(root);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(preview));
                using var output = File.Create(Path.Combine(AppContext.BaseDirectory, "preview-" + skin + ".png"));
                encoder.Save(output);
            }
            window.Close();
            results.Add("PASS: carga de XAML. NO verifica UI interactiva, WhatsApp, notificaciones, Spotify ni audio.");
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "self-test-results.txt"), results);
            return 0;
        }
        catch (Exception ex)
        {
            results.Add("FAIL: " + ex);
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "self-test-results.txt"), results);
            return 1;
        }
    }

    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
