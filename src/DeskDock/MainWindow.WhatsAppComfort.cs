using System.Windows;
using System.Windows.Interop;

namespace DeskDock;

public partial class MainWindow
{
    private bool _comfortReady;
    private Rect? _compactWhatsAppBounds;
    private Rect _appliedWhatsAppBounds;
    internal Rect SavedCompactBounds => _compactWhatsAppBounds ??
        (WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds);

    private void InitializeWhatsAppComfort(Settings settings)
    {
        WhatsAppComfortOption.IsChecked = settings.ComfortableWhatsApp;
        _comfortReady = true;
    }

    internal void UpdateWhatsAppComfort()
    {
        if (!_comfortReady || !IsLoaded || WindowState != WindowState.Normal) return;
        var current = new Rect(Left, Top, Width, Height);
        var work = CurrentWorkArea();
        if (MainTabs.SelectedIndex == 0 && WhatsAppComfortOption.IsChecked)
        {
            if (_compactWhatsAppBounds.HasValue) return;
            _compactWhatsAppBounds = current;
            _appliedWhatsAppBounds = WhatsAppComfortBounds.Expand(current, work);
            ApplyComfortBounds(_appliedWhatsAppBounds);
        }
        else if (_compactWhatsAppBounds is Rect compact)
        {
            _compactWhatsAppBounds = null;
            ApplyComfortBounds(WhatsAppComfortBounds.Restore(compact, _appliedWhatsAppBounds, current, work));
        }
    }

    private Rect CurrentWorkArea()
    {
        var screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle).WorkingArea;
        var transform = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformFromDevice;
        return transform.HasValue ? new System.Windows.Media.MatrixTransform(transform.Value)
            .TransformBounds(new Rect(screen.X, screen.Y, screen.Width, screen.Height)) : SystemParameters.WorkArea;
    }

    private void ApplyComfortBounds(Rect bounds)
    {
        Left = bounds.Left; Top = bounds.Top; Width = bounds.Width; Height = bounds.Height;
    }

    private void WhatsAppComfortClick(object sender, RoutedEventArgs e)
    {
        UpdateWhatsAppComfort();
        SaveSettings();
    }
}
