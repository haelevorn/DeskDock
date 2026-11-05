using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace DeskDock;

public partial class MainWindow
{
    internal PomodoroTimer Pomodoro { get; } = new();
    private readonly DispatcherTimer _focusTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private long _lastNotificationPoll;

    private void InitializeFocusTools(Settings settings)
    {
        Pomodoro.Configure(settings.Pomodoro ?? new());
        FocusMinutes.Text = Pomodoro.Options.FocusMinutes.ToString(CultureInfo.InvariantCulture);
        ShortMinutes.Text = Pomodoro.Options.ShortBreakMinutes.ToString(CultureInfo.InvariantCulture);
        LongMinutes.Text = Pomodoro.Options.LongBreakMinutes.ToString(CultureInfo.InvariantCulture);
        RoundCount.Text = Pomodoro.Options.SessionsBeforeLongBreak.ToString(CultureInfo.InvariantCulture);
        AutoStages.IsChecked = Pomodoro.Options.AutoAdvance;
        if (!_testMode)
        {
            Width = SafeDimension(settings.WindowWidth, 360, 300, SystemParameters.WorkArea.Width);
            Height = SafeDimension(settings.WindowHeight, 520, 340, SystemParameters.WorkArea.Height);
        }
        _focusTimer.Tick += FocusTimerTick;
        UpdateClock();
        RenderPomodoro();
    }

    internal static double SafeDimension(double value, double fallback, double minimum, double screen) =>
        Math.Clamp(double.IsFinite(value) ? value : fallback, minimum, Math.Max(minimum, screen));

    private void FocusTimerTick(object? sender, EventArgs e)
    {
        UpdateClock();
        UpdatePomodoro();
        if (Environment.TickCount64 - _lastNotificationPoll >= 2000)
        {
            _lastNotificationPoll = Environment.TickCount64;
            try { CheckUnreadTitle(); }
            catch (Exception ex) { Diagnostics.Record("NotificationCounterPoll", ex); }
        }
    }

    private void UpdateClock()
    {
        var local = DateTime.Now;
        ClockText.Text = local.ToString("HH:mm", CultureInfo.CurrentCulture);
        ClockText.ToolTip = local.ToString("F", CultureInfo.CurrentCulture);
    }

    private void UpdatePomodoro()
    {
        var finished = Pomodoro.Tick();
        if (finished.HasValue)
        {
            string next = PomodoroTimer.StageName(Pomodoro.Stage);
            PomoMessage.Text = $"Terminó {PomodoroTimer.StageName(finished.Value).ToLowerInvariant()}. " +
                (Pomodoro.IsRunning ? $"Ahora: {next.ToLowerInvariant()}." : $"Siguiente: {next.ToLowerInvariant()}. Pulsa Iniciar.");
            ShowNotice(PomoMessage.Text, title: "DeskDock · Pomodoro", targetTab: 2);
        }
        RenderPomodoro();
    }

    internal void RenderPomodoro()
    {
        PomoStage.Text = PomodoroTimer.StageName(Pomodoro.Stage);
        int total = (int)Math.Ceiling(Pomodoro.Remaining.TotalSeconds);
        PomoTime.Text = $"{total / 60:00}:{total % 60:00}";
        PomoSessions.Text = $"Sesiones: {Pomodoro.CompletedFocusSessions} · descanso largo cada {Pomodoro.Options.SessionsBeforeLongBreak}";
        PomoProgress.Value = Math.Clamp(100 * (1 - Pomodoro.Remaining.TotalSeconds / Pomodoro.Duration.TotalSeconds), 0, 100);
        PomoStart.Content = Pomodoro.IsRunning ? "Pausar" : "Iniciar";
        PomoBadge.Visibility = Pomodoro.IsRunning ? Visibility.Visible : Visibility.Collapsed;
        PomoBadgeText.Text = $"◷ {PomoTime.Text} · {PomoStage.Text}";
        _ultraMini?.UpdateStatus(ClockText.Text, Pomodoro.IsRunning ? $"{PomoTime.Text} · {PomoStage.Text}" : null);
    }

    private void PomoBadgeClick(object sender, RoutedEventArgs e) => MainTabs.SelectedIndex = 2;

    private void PomoStartClick(object sender, RoutedEventArgs e)
    {
        UpdatePomodoro();
        if (Pomodoro.IsRunning) Pomodoro.Pause();
        else Pomodoro.Start();
        RenderPomodoro();
    }

    private void PomoResetClick(object sender, RoutedEventArgs e)
    {
        Pomodoro.Reset();
        PomoMessage.Text = "Ciclo reiniciado. Pulsa Iniciar cuando estés listo.";
        RenderPomodoro();
    }

    private void ApplyPomodoroClick(object sender, RoutedEventArgs e)
    {
        if (!TryReadPomodoro(out var options)) return;
        Pomodoro.Configure(options);
        SaveSettings();
        PomoMessage.Text = "Etapas guardadas. Pulsa Iniciar para comenzar.";
        RenderPomodoro();
    }

    private bool TryReadPomodoro(out PomodoroOptions options)
    {
        options = new();
        if (!ReadMinutes(FocusMinutes.Text, 1, 240, out int focus) || !ReadMinutes(ShortMinutes.Text, 1, 60, out int shortBreak) ||
            !ReadMinutes(LongMinutes.Text, 1, 120, out int longBreak) || !ReadMinutes(RoundCount.Text, 2, 12, out int rounds))
        {
            PomoMessage.Text = "Usa números enteros: concentración 1–240 min, corto 1–60, largo 1–120 y sesiones 2–12.";
            return false;
        }
        options = new(focus, shortBreak, longBreak, rounds, AutoStages.IsChecked == true);
        return true;
    }

    internal static bool ReadMinutes(string value, int min, int max, out int number) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number >= min && number <= max;

    private void CompactWindowClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Normal;
        Width = 320;
        Height = 420;
        SaveSettings();
    }

    private void ResponsiveSizeChanged(object sender, SizeChangedEventArgs e)
    {
        WindowCorners.Apply(this, 16);
        if (SpotifyCover is null || MiniPlayer is null) return;
        bool narrow = ActualWidth < 350;
        MiniArtworkFrame.Visibility = narrow ? Visibility.Collapsed : Visibility.Visible;
        MiniArtColumn.Width = new GridLength(narrow ? 0 : 38);
        SpotifyCover.Width = SpotifyCover.Height = Math.Clamp((ActualHeight - 260) * 0.5, 48, 180);
        FullTrackTitle.FontSize = ActualHeight < 480 ? 16 : 21;
        SpotifyContent.Margin = new Thickness(12, ActualHeight < 480 ? 6 : 12, 12, 6);
    }
    private void MainDpiChanged(object sender, DpiChangedEventArgs e) =>
        Dispatcher.BeginInvoke(new Action(() => WindowCorners.Apply(this, 16)));
}
