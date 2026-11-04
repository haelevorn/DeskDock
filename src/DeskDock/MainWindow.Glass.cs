using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace DeskDock;

public partial class MainWindow
{
    internal static bool GlassSupported => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
    [StructLayout(LayoutKind.Sequential)]
    private struct GlassMargins { public int Left, Right, Top, Bottom; }
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr window, ref GlassMargins margins);

    private void GlassClick(object sender, RoutedEventArgs e) { ApplyGlass(); SaveSettings(); }

    private void ApplyGlass()
    {
        if (GlassOption is null) return;
        GlassOption.IsEnabled = GlassSupported;
        GlassOption.ToolTip = GlassSupported ? "Acrílico de Windows en el marco y paneles nativos" : "Requiere Windows 11 22H2 o posterior";
        bool enabled = GlassSupported && GlassOption.IsChecked && !_testMode;
        if (!GlassSupported) GlassOption.IsChecked = false;
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero && GlassSupported && !_testMode)
        {
            int backdrop = enabled ? 3 : 1; // DWMSBT_TRANSIENTWINDOW / DWMSBT_NONE
            int result = DwmSetWindowAttribute(handle, 38, ref backdrop, sizeof(int));
            if (result < 0)
            {
                enabled = false;
                GlassOption.IsChecked = false;
                WebStatus.Text = "Windows no pudo activar Glass. Se mantiene el skin opaco.";
            }
            if (WindowChrome.GetWindowChrome(this) is { } chrome) chrome.GlassFrameThickness = new Thickness(enabled ? -1 : 0);
            if (HwndSource.FromHwnd(handle)?.CompositionTarget is { } target) target.BackgroundColor = enabled ? Colors.Transparent : Colors.Black;
            var margins = new GlassMargins { Left = enabled ? -1 : 0, Right = enabled ? -1 : 0, Top = enabled ? -1 : 0, Bottom = enabled ? -1 : 0 };
            _ = DwmExtendFrameIntoClientArea(handle, ref margins);
        }
        if (enabled)
        {
            Background = GlassBrush("WindowBackground", 0.64);
            MainTabs.Background = GlassBrush("SurfaceBackground", 0.70);
            MiniPlayer.Background = GlassBrush("PlayerBackground", 0.80);
        }
        else
        {
            SetResourceReference(BackgroundProperty, "WindowBackground");
            MainTabs.SetResourceReference(Control.BackgroundProperty, "SurfaceBackground");
            MiniPlayer.SetResourceReference(Border.BackgroundProperty, "PlayerBackground");
        }
    }

    private static Brush GlassBrush(string key, double opacity)
    {
        var brush = ((Brush)Application.Current.Resources[key]).CloneCurrentValue();
        brush.Opacity *= opacity;
        return brush;
    }
}
