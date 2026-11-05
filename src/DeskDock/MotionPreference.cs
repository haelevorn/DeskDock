using System;
using System.ComponentModel;
using System.Windows;

namespace DeskDock;

public sealed class MotionPreference
{
    private bool? _testOverride;
    public bool Enabled { get; private set; } = SystemParameters.ClientAreaAnimation;
    internal event EventHandler? Changed;
    internal static MotionPreference Current => (MotionPreference)Application.Current.FindResource("MotionPreference");

    public MotionPreference() => SystemParameters.StaticPropertyChanged += SystemPreferenceChanged;
    private void SystemPreferenceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SystemParameters.ClientAreaAnimation)) return;
        if (Application.Current.Dispatcher.CheckAccess()) Refresh();
        else Application.Current.Dispatcher.BeginInvoke(new Action(Refresh));
    }
    internal void OverrideForTests(bool? value) { _testOverride = value; Refresh(); }
    private void Refresh()
    {
        bool enabled = _testOverride ?? SystemParameters.ClientAreaAnimation;
        if (Enabled == enabled) return;
        Enabled = enabled; Changed?.Invoke(this, EventArgs.Empty);
    }
}
