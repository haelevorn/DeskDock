using System;
using System.ComponentModel;
using System.Windows;

namespace DeskDock;

public partial class UltraMiniWindow : Window
{
    private readonly Action _restore, _pomodoro;
    private readonly Action<MediaCommand> _command;
    private bool _exiting;
    internal UltraMiniWindow(PlayerState player, Action restore, Action pomodoro, Action<MediaCommand> command)
    {
        InitializeComponent();
        DataContext = player;
        _restore = restore; _pomodoro = pomodoro; _command = command;
    }
    internal void UpdateStatus(string clock, string? pomodoro)
    {
        MiniClock.Text = clock;
        MiniPomodoro.Content = pomodoro;
        MiniPomodoro.Visibility = pomodoro is null ? Visibility.Collapsed : Visibility.Visible;
    }
    private void RestoreClick(object sender, RoutedEventArgs e) => _restore();
    private void PomodoroClick(object sender, RoutedEventArgs e) => _pomodoro();
    private void PlayClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Play);
    private void PauseClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Pause);
    private void PreviousClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Previous);
    private void NextClick(object sender, RoutedEventArgs e) => _command(MediaCommand.Next);
    private void MiniClosing(object? sender, CancelEventArgs e) { if (!_exiting) { e.Cancel = true; _restore(); } }
    internal void CloseForExit() { _exiting = true; Close(); }
}
