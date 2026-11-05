using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DeskDock;

internal static class WhatsAppComfortSelfTests
{
    internal static void Run(List<string> results)
    {
        var work = new Rect(0, 0, 1366, 728);
        var compact = new Rect(1000, 300, 320, 420);
        var expanded = WhatsAppComfortBounds.Expand(compact, work);
        Check(expanded.Size == new Size(600, 600) && work.Contains(expanded), "WhatsApp cómodo cabe dentro del escritorio sin tapar la barra de tareas", results);
        Check(WhatsAppComfortBounds.Restore(compact, expanded, expanded, work) == compact,
            "Salir de WhatsApp deshace el desplazamiento necesario para ampliar cerca del borde", results);
        var moved = new Rect(200, 50, 600, 600);
        Check(WhatsAppComfortBounds.Restore(compact, expanded, moved, work).Location == moved.Location,
            "Mover WhatsApp manualmente conserva la nueva posición al volver al tamaño compacto", results);
        var negativeMonitor = new Rect(-1280, -100, 1280, 720);
        Check(negativeMonitor.Contains(WhatsAppComfortBounds.Expand(new Rect(-340, 100, 320, 420), negativeMonitor)),
            "Ampliación respeta monitores con coordenadas negativas", results);
        Check(WhatsAppComfortBounds.Expand(new Rect(0, 0, 320, 420), new Rect(0, 0, 640, 480)).Height == 480,
            "Pantalla baja limita la altura de la vista cómoda", results);
        var wide = new Rect(0, 0, 900, 650);
        Check(WhatsAppComfortBounds.Expand(wide, work) == wide, "Una ventana ya amplia no se encoge al entrar a WhatsApp", results);
        Check(JsonSerializer.Deserialize<Settings>("{}")!.ComfortableWhatsApp &&
              !JsonSerializer.Deserialize<Settings>(JsonSerializer.Serialize(new Settings(ComfortableWhatsApp: false)))!.ComfortableWhatsApp,
            "Preferencia de ampliación migra activada y conserva desactivación explícita", results);

        var window = new MainWindow(testMode: true) { ShowInTaskbar = false, Opacity = 0 };
        try
        {
            window.Show();
            window.Width = 320; window.Height = 420;
            window.Left = SystemParameters.WorkArea.Left + 20; window.Top = SystemParameters.WorkArea.Top + 20;
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            var tabs = (TabControl)window.FindName("MainTabs");
            var option = (MenuItem)window.FindName("WhatsAppComfortOption");
            option.IsChecked = true; tabs.SelectedIndex = 0;
            window.UpdateWhatsAppComfort();
            window.UpdateWhatsAppComfort();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            Check(window.Width >= 600 && window.Height >= 600 && window.SavedCompactBounds.Size == new Size(320, 420),
                "Ventana real amplía WhatsApp una vez y guarda el tamaño compacto, sin acumular crecimiento", results);
            tabs.SelectedIndex = 2; window.UpdateWhatsAppComfort();
            Check(window.Width == 320 && window.Height == 420, "Volver a Pomodoro restaura el tamaño anterior exacto", results);
            option.IsChecked = false; tabs.SelectedIndex = 0; window.UpdateWhatsAppComfort();
            Check(window.Width == 320 && window.Height == 420, "Desactivar ampliación mantiene WhatsApp compacto", results);
            option.IsChecked = true; window.UpdateWhatsAppComfort();
            window.WindowState = WindowState.Maximized;
            tabs.SelectedIndex = 1; window.UpdateWhatsAppComfort();
            Check(window.WindowState == WindowState.Maximized && window.SavedCompactBounds.Width == 320,
                "Cambiar pestaña no desmaximiza la ventana ni pierde el tamaño guardado", results);
            window.WindowState = WindowState.Normal;
            window.UpdateWhatsAppComfort();
            Check(window.Width == 320 && window.Height == 420, "Restaurar después de maximizar recupera el tamaño compacto pendiente", results);
        }
        finally { window.Close(); }
    }

    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
