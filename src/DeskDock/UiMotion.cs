using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DeskDock;

public static class UiMotion
{
    public static readonly DependencyProperty InteractiveProperty = DependencyProperty.RegisterAttached("Interactive", typeof(bool), typeof(UiMotion),
        new PropertyMetadata(false, InteractiveChanged));
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached("State", typeof(State), typeof(UiMotion));
    public static bool GetInteractive(DependencyObject value) => (bool)value.GetValue(InteractiveProperty);
    public static void SetInteractive(DependencyObject value, bool enabled) => value.SetValue(InteractiveProperty, enabled);
    private static void InteractiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control control) return;
        if (control.GetValue(StateProperty) is State previous) previous.Dispose();
        control.ClearValue(StateProperty);
        if ((bool)e.NewValue && (control is Button || control is TabItem)) control.SetValue(StateProperty, new State(control));
    }

    private sealed class State : IDisposable
    {
        private readonly Control _control;
        private DependencyPropertyDescriptor? _descriptor;
        private MotionPreference? _preference;
        private Border? _hover, _selection, _indicator;
        private FrameworkElement? _surface;
        private bool _attached;
        internal State(Control control)
        {
            _control = control;
            control.Loaded += Loaded; control.Unloaded += Unloaded;
            if (control.IsLoaded) Attach();
        }
        private void Loaded(object sender, RoutedEventArgs e) => Attach();
        private void Unloaded(object sender, RoutedEventArgs e) => Detach();
        private void Attach()
        {
            if (_attached) return;
            _control.ApplyTemplate();
            _surface = _control.Template.FindName("MotionSurface", _control) as FrameworkElement;
            if (_surface is null) return;
            _hover = _control.Template.FindName("MotionHover", _control) as Border;
            _selection = _control.Template.FindName("MotionSelection", _control) as Border;
            _indicator = _control.Template.FindName("TabIndicator", _control) as Border;
            if (_surface.RenderTransform is ScaleTransform scale) _surface.RenderTransform = scale.Clone();
            if (_indicator?.RenderTransform is TranslateTransform translation) _indicator.RenderTransform = translation.Clone();
            _preference = MotionPreference.Current;
            _preference.Changed += PreferenceChanged;
            _control.MouseEnter += PointerChanged; _control.MouseLeave += PointerChanged;
            _control.IsEnabledChanged += EnabledChanged; _control.IsVisibleChanged += VisibleChanged;
            _control.GotKeyboardFocus += FocusChanged; _control.LostKeyboardFocus += FocusChanged;
            var property = _control is Button ? ButtonBase.IsPressedProperty : TabItem.IsSelectedProperty;
            _descriptor = DependencyPropertyDescriptor.FromProperty(property, _control.GetType());
            _descriptor.AddValueChanged(_control, ValueChanged);
            _attached = true; Apply(false);
        }
        private void Detach()
        {
            if (!_attached) return;
            Apply(false);
            _preference!.Changed -= PreferenceChanged;
            _control.MouseEnter -= PointerChanged; _control.MouseLeave -= PointerChanged;
            _control.IsEnabledChanged -= EnabledChanged; _control.IsVisibleChanged -= VisibleChanged;
            _control.GotKeyboardFocus -= FocusChanged; _control.LostKeyboardFocus -= FocusChanged;
            _descriptor!.RemoveValueChanged(_control, ValueChanged);
            _attached = false;
        }
        private void PointerChanged(object sender, MouseEventArgs e) => Apply(true);
        private void FocusChanged(object sender, KeyboardFocusChangedEventArgs e) => Apply(true);
        private void ValueChanged(object? sender, EventArgs e) => Apply(true);
        private void PreferenceChanged(object? sender, EventArgs e) => Apply(false);
        private void EnabledChanged(object sender, DependencyPropertyChangedEventArgs e) => Apply(false);
        private void VisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => Apply(false);
        private void Apply(bool animate)
        {
            bool allowed = _preference?.Enabled == true;
            animate &= allowed && _control.IsVisible && _control.IsEnabled;
            bool pressed = _control is Button button && button.IsPressed && _control.IsEnabled;
            bool hover = _control.IsEnabled && (_control.IsMouseOver || _control.IsKeyboardFocusWithin);
            bool selected = _control is TabItem tab && tab.IsSelected;
            if (_hover is not null) Fade(_hover, pressed ? .16 : hover ? .09 : 0, animate);
            if (_selection is not null) Fade(_selection, selected ? 1 : 0, animate);
            if (_indicator is not null)
            {
                Fade(_indicator, selected ? 1 : 0, animate);
                if (_indicator.RenderTransform is TranslateTransform translation)
                    Tween(translation, TranslateTransform.XProperty, selected ? 0 : -3, animate);
            }
            if (_surface?.RenderTransform is ScaleTransform scale)
            {
                double size = allowed && pressed ? .96 : 1;
                Tween(scale, ScaleTransform.ScaleXProperty, size, animate);
                Tween(scale, ScaleTransform.ScaleYProperty, size, animate);
            }
        }
        private static void Fade(Border target, double value, bool animate) => Tween(target, UIElement.OpacityProperty, value, animate);
        private static void Tween(IAnimatable target, DependencyProperty property, double value, bool animate)
        {
            var d = (DependencyObject)target;
            double current = (double)d.GetValue(property);
            target.BeginAnimation(property, null);
            d.SetValue(property, value);
            if (!animate || Math.Abs(current - value) < .001) return;
            var animation = new DoubleAnimation(current, value, TimeSpan.FromMilliseconds(160))
            { FillBehavior = FillBehavior.Stop, EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            target.BeginAnimation(property, animation, HandoffBehavior.SnapshotAndReplace);
        }
        public void Dispose()
        {
            Detach(); _control.Loaded -= Loaded; _control.Unloaded -= Unloaded;
        }
    }
}
