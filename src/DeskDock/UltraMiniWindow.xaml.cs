using System;
using System.ComponentModel;
using System.Windows;

namespace DeskDock;

public partial class UltraMiniWindow : Window
{
    private readonly Action _restore, _pomodoro;
    private readonly Action<MediaCommand> _command;
    private readonly Action<double>? _setVolume;
    private readonly Action? _toggleMute;
    private readonly Action? _openSpotify;
    private bool _syncingAudio, _audioAvailable;
    private bool _exiting;
    internal UltraMiniWindow(PlayerState player, Action restore, Action pomodoro, Action<MediaCommand> command, Action<double>? setVolume = null, Action? toggleMute = null, Action? openSpotify = null)
    {
        InitializeComponent();
        DataContext = player;
        _restore = restore; _pomodoro = pomodoro; _command = command;
        _setVolume = setVolume; _toggleMute = toggleMute;
        _openSpotify = openSpotify;
        UpdateAudio(0, false, false);
    }
    internal void UpdateStatus(string clock, string? pomodoro, PomodoroTimer? timer = null)
    {
        MiniClock.Text = clock;
        bool wasVisible = MiniPomodoro.Visibility == Visibility.Visible;
        MiniPomodoroText.Text = pomodoro;
        if (timer is not null) MiniPomodoroStages.SetTimer(timer);
        MiniPomodoro.Visibility = pomodoro is null ? Visibility.Collapsed : Visibility.Visible;
        if (wasVisible != (pomodoro is not null))
        {
            double targetHeight = Height + (pomodoro is null ? -19 : 19);
            MinHeight = pomodoro is null ? 94 : 113;
            Height = Math.Max(MinHeight, targetHeight);
        }
    }
    internal void UpdateAudio(double volume, bool muted, bool available)
    {
        _syncingAudio = true;
        try
        {
            _audioAvailable = available;
            if (!MiniMasterVolume.IsMouseCaptureWithin && !MiniMasterVolume.IsKeyboardFocusWithin)
                MiniMasterVolume.Value = Math.Clamp(volume, 0, 100);
            MiniMasterVolume.IsEnabled = MiniMute.IsEnabled = available;
            MiniVolumeValue.Text = available ? $"Volumen maestro · {volume:0}%" : "Sin dispositivo de audio";
            MiniMute.Header = muted ? "Activar sonido" : "Silenciar";
            MiniVolumeButton.Content = muted ? "\uE74F" : "\uE767";
        }
        finally { _syncingAudio = false; }
    }
    private void MiniVolumeClick(object sender, RoutedEventArgs e)
    {
        MiniVolumeButton.ContextMenu.PlacementTarget = MiniVolumeButton;
        MiniVolumeButton.ContextMenu.IsOpen = true;
    }
    private void MiniVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_syncingAudio && _audioAvailable && IsLoaded) _setVolume?.Invoke(e.NewValue);
    }
    private void MiniMuteClick(object sender, RoutedEventArgs e) { if (_audioAvailable) _toggleMute?.Invoke(); }
    private void RestoreClick(object sender, RoutedEventArgs e) => _restore();
    private void OpenSpotifyClick(object sender, RoutedEventArgs e) => _openSpotify?.Invoke();
    private void PomodoroClick(object sender, RoutedEventArgs e) => _pomodoro();
    private void PlayClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Play);
    private void PauseClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Pause);
    private void PreviousClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Previous);
    private void NextClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Next);
    private void MiniClosing(object? sender, CancelEventArgs e) { if (!_exiting) { e.Cancel = true; _restore(); } }
    private void MiniShapeChanged(object? sender, EventArgs e) => WindowCorners.Apply(this, 13);
    private void MiniSizeChanged(object sender, SizeChangedEventArgs e) => WindowCorners.Apply(this, 13);
    private void MiniDpiChanged(object sender, DpiChangedEventArgs e) => Dispatcher.BeginInvoke(new Action(() => WindowCorners.Apply(this, 13)));
    internal void CloseForExit() { _exiting = true; Close(); }
}
