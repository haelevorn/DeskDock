using System;
using System.Windows;

namespace DeskDock;

internal static class WhatsAppComfortBounds
{
    internal static Rect Expand(Rect current, Rect work) => Fit(new Rect(current.X, current.Y,
        Math.Max(current.Width, 600), Math.Max(current.Height, 600)), work);

    internal static Rect Fit(Rect desired, Rect work)
    {
        double width = Math.Min(desired.Width, work.Width), height = Math.Min(desired.Height, work.Height);
        return new Rect(Math.Clamp(desired.Left, work.Left, work.Right - width),
            Math.Clamp(desired.Top, work.Top, work.Bottom - height), width, height);
    }

    internal static Rect Restore(Rect compact, Rect applied, Rect current, Rect work)
    {
        bool moved = Math.Abs(current.Left - applied.Left) > 2 || Math.Abs(current.Top - applied.Top) > 2;
        return Fit(new Rect(moved ? current.Location : compact.Location, compact.Size), work);
    }
}
