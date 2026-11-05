using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeskDock;

internal static class MotionSelfTests
{
    internal static void Run(List<string> results)
    {
        var preference = MotionPreference.Current;
        preference.OverrideForTests(true);
        TimeSpan now = TimeSpan.Zero;
        var timer = new PomodoroTimer(() => now);
        timer.Configure(new(1, 1, 1, 4));
        var stages = new PomodoroStagesView(); stages.SetTimer(timer);
        int clicks = 0;
        var button = new Button { Content = "Prueba", Width = 100, Height = 32 };
        button.Click += (_, _) => clicks++;
        var first = new TabItem { Header = "A" };
        var second = new TabItem { Header = "B" };
        var tabs = new TabControl(); tabs.Items.Add(first); tabs.Items.Add(second);
        var panel = new StackPanel(); panel.Children.Add(button); panel.Children.Add(tabs); panel.Children.Add(stages);
        var window = new Window { Width = 300, Height = 220, Content = panel, ShowInTaskbar = false, Opacity = 0 };
        try
        {
            window.Show();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            Check(!stages.IsStageAnimating, "Pomodoro no anima su carga inicial", results);
            tabs.SelectedIndex = 1;
            var selection = (Border)second.Template.FindName("MotionSelection", second);
            var indicator = (Border)second.Template.FindName("TabIndicator", second);
            Check(selection.HasAnimatedProperties && indicator.HasAnimatedProperties,
                "Cambiar pestaña inicia fundido y movimiento del indicador", results);
            WaitForFrames(300);
            Check(selection.Opacity == 1 && indicator.Opacity == 1 && ((TranslateTransform)indicator.RenderTransform).X == 0,
                "Transición de pestaña termina en su selección estable", results);

            double width = button.ActualWidth, height = button.ActualHeight;
            button.Focus();
            var source = PresentationSource.FromVisual(button)!;
            button.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Space) { RoutedEvent = Keyboard.KeyDownEvent });
            var surface = (FrameworkElement)button.Template.FindName("MotionSurface", button);
            var scale = (ScaleTransform)surface.RenderTransform;
            Check(button.IsPressed && (double)scale.GetAnimationBaseValue(ScaleTransform.ScaleXProperty) == .96,
                "Presión por teclado anima el botón sin depender del ratón", results);
            button.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Space) { RoutedEvent = Keyboard.KeyUpEvent });
            WaitForFrames(250);
            Check(clicks == 1 && scale.ScaleX == 1 && scale.ScaleY == 1 && button.ActualWidth == width && button.ActualHeight == height,
                "Soltar restaura el botón, ejecuta una acción y conserva su área de interacción", results);

            timer.Start(); now += TimeSpan.FromMinutes(1); timer.Tick(); stages.SetTimer(timer);
            Check(stages.IsStageAnimating && timer.CycleResults[0] == PomodoroStageResult.Completed,
                "Fin de etapa inicia relleno suave y pulso único sin alterar el temporizador", results);
            WaitForFrames(300);
            Check(!stages.IsStageAnimating && stages.StageAnimationProgress == 1 && timer.CycleStageIndex == 1,
                "Animación de etapa termina sin repetirse ni avanzar otra etapa", results);
            timer.NextStage(); stages.SetTimer(timer);
            Check(stages.IsStageAnimating && timer.CycleResults[1] == PomodoroStageResult.Skipped,
                "Salto manual conserva la raya y anima solo la llegada a la siguiente etapa", results);
            stages.Visibility = Visibility.Collapsed;
            Check(!stages.IsStageAnimating, "Ocultar las etapas cancela sus animaciones", results);
            stages.Visibility = Visibility.Visible; stages.SetTimer(timer);
            Check(!stages.IsStageAnimating, "Volver a mostrar no repite la animación de una etapa ya procesada", results);

            tabs.SelectedIndex = 0;
            timer.NextStage(); stages.SetTimer(timer);
            preference.OverrideForTests(false);
            var firstSelection = (Border)first.Template.FindName("MotionSelection", first);
            Check(!firstSelection.HasAnimatedProperties && firstSelection.Opacity == 1 && !stages.IsStageAnimating,
                "Reducir animaciones cancela transiciones activas y deja el estado final visible", results);
            tabs.SelectedIndex = 1; timer.NextStage(); stages.SetTimer(timer);
            Check(!selection.HasAnimatedProperties && selection.Opacity == 1 && !stages.IsStageAnimating,
                "Con animaciones desactivadas pestañas y etapas responden al instante", results);
        }
        finally { window.Close(); preference.OverrideForTests(null); }
    }

    private static void WaitForFrames(int milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start(); Dispatcher.PushFrame(frame);
    }
    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
