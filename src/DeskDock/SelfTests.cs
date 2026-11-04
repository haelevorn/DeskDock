using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace DeskDock;

// Pruebas sin cuentas, navegador, altavoces o Spotify. Las integraciones requieren prueba manual.
internal static class SelfTests
{
    public static int Run()
    {
        var results = new List<string>();
        try
        {
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
            var settings = new Settings(true, false);
            Check(JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(settings)) == settings, "Persistencia de opciones", results);
            Check(NotificationPolicy.UnreadCount("(3) WhatsApp") == 3, "Contador de no leídos", results);
            Check(NotificationPolicy.UnreadCount("WhatsApp") == 0, "Contador reiniciado", results);
            foreach (string title in new[] { "(3) Otro sitio", "WhatsApp - contacto", "(texto) WhatsApp", "(999999999999999) WhatsApp" })
                Check(NotificationPolicy.UnreadCount(title) is null, "Ignorar título no reconocido", results);

            // Carga el XAML real con sus controles sin iniciar servicios ni mostrar la ventana.
            var window = new MainWindow(testMode: true) { ShowInTaskbar = false, Opacity = 0 };
            window.Show();
            window.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Check(((System.Windows.FrameworkElement)window.FindName("MiniPlayer")).ActualHeight <= 60,
                "Mini reproductor compacto sin filas extra", results);
            Check(((System.Windows.FrameworkElement)window.FindName("Web")).ActualHeight > window.ActualHeight * 0.70,
                "WhatsApp ocupa más del 70% del alto inicial de la ventana", results);
            Check(window.MinWidth >= 390 && window.MinHeight >= 500, "Dimensiones mínimas", results);
            Check(window.FindName("Web") is Microsoft.Web.WebView2.Wpf.WebView2, "WebView2 WPF en XAML", results);
            Check(window.FindName("MasterVolume") is System.Windows.Controls.Slider, "Control de volumen en XAML", results);
            Check(window.FindName("Play") is System.Windows.Controls.Button, "Control de reproducción en XAML", results);
            Check(window.WindowStyle == WindowStyle.None, "Sin barra de título estándar", results);
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
