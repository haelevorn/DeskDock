using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DeskDock;

internal static class WindowCorners
{
    // Recortar el HWND real, incluido WebView2, al mismo radio que su borde WPF.
    // Se conserva una ventana opaca; no se usa AllowsTransparency ni acrílico.
    public static void Apply(Window window, double radius)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero || window.WindowState == WindowState.Minimized) return;
        if (window.WindowState == WindowState.Maximized) { SetWindowRgn(handle, IntPtr.Zero, true); return; }
        if (!GetWindowRect(handle, out var bounds)) return;
        int width = bounds.Right - bounds.Left, height = bounds.Bottom - bounds.Top;
        if (width < 1 || height < 1) return;
        var dpi = VisualTreeHelper.GetDpi(window);
        int diameterX = Math.Max(2, (int)Math.Round(radius * dpi.DpiScaleX * 2));
        int diameterY = Math.Max(2, (int)Math.Round(radius * dpi.DpiScaleY * 2));
        IntPtr region = CreateRoundRectRgn(0, 0, width + 1, height + 1, diameterX, diameterY);
        if (region == IntPtr.Zero) return;
        // Tras éxito, Windows es propietario de la región y libera la anterior.
        if (SetWindowRgn(handle, region, true) == 0) DeleteObject(region);
    }

    internal static bool HasClippedCorner(Window window)
    {
        IntPtr region = CreateRectRgn(0, 0, 0, 0);
        if (region == IntPtr.Zero) return false;
        try
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (GetWindowRgn(handle, region) <= 0 || !GetWindowRect(handle, out var bounds)) return false;
            return !PtInRegion(region, 0, 0) && PtInRegion(region, (bounds.Right - bounds.Left) / 2, (bounds.Bottom - bounds.Top) / 2);
        }
        finally { DeleteObject(region); }
    }

    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out Rect rect);
    [DllImport("user32.dll")] private static extern int SetWindowRgn(IntPtr window, IntPtr region, bool redraw);
    [DllImport("user32.dll")] private static extern int GetWindowRgn(IntPtr window, IntPtr region);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int ellipseWidth, int ellipseHeight);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);
    [DllImport("gdi32.dll")] private static extern bool PtInRegion(IntPtr region, int x, int y);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
}
