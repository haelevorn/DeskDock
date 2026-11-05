using System.Windows;

namespace DeskDock;

public partial class MainWindow
{
    private UltraMiniWindow? _ultraMini;
    private void EnterUltraMiniClick(object sender, RoutedEventArgs e)
    {
        _ultraMini ??= new UltraMiniWindow(Player, Restore,
            () => { Restore(); MainTabs.SelectedIndex = 2; }, async command => await SendAsync(command));
        _ultraMini.Topmost = Topmost;
        _ultraMini.UpdateStatus(ClockText.Text, Pomodoro.IsRunning ? $"{PomoTime.Text} · {PomoStage.Text}" : null);
        _ultraMini.Show();
        Hide();
        _ultraMini.Activate();
    }
}
