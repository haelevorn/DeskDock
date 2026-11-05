using System.Windows;

namespace DeskDock;

public partial class MainWindow
{
    private UltraMiniWindow? _ultraMini;
    private void EnterUltraMiniClick(object sender, RoutedEventArgs e)
    {
        _ultraMini ??= new UltraMiniWindow(Player, Restore,
            () => { Restore(); MainTabs.SelectedIndex = 2; }, async command => await SendAsync(command),
            SetMiniVolume, () => MuteClick(this, new RoutedEventArgs()));
        _ultraMini.Topmost = Topmost;
        _ultraMini.UpdateStatus(ClockText.Text, Pomodoro.IsRunning ? $"{PomoTime.Text} · {PomoStage.Text}" : null);
        _ultraMini.Show();
        RefreshAudio();
        Hide();
        _ultraMini.Activate();
    }
    private void SetMiniVolume(double volume)
    {
        try { _audio.SetVolume((float)(volume / 100)); }
        catch (System.Exception ex) { Diagnostics.Record("MiniAudioVolume", ex); }
        RefreshAudio();
    }
}
