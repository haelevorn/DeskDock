using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DeskDock;

public partial class MainWindow
{
    private PomodoroProfiles _profiles = new();
    private void RefreshProfiles(PomodoroProfile? selected = null)
    {
        ProfilePicker.ItemsSource = _profiles.Items.ToArray();
        ProfilePicker.SelectedItem = selected;
    }
    private void ProfileSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ProfilePicker.SelectedItem is PomodoroProfile profile) ProfileName.Text = profile.Name;
    }
    private void SaveProfileClick(object sender, RoutedEventArgs e)
    {
        if (!TryReadPomodoro(out var options)) return;
        try
        {
            var profile = _profiles.Upsert(ProfileName.Text, options);
            RefreshProfiles(profile);
            SaveSettings();
            PomoMessage.Text = $"Perfil «{profile.Name}» guardado. Cargar y reiniciar lo aplica al temporizador.";
        }
        catch (ArgumentException ex) { PomoMessage.Text = ex.Message; }
    }
    private void LoadProfileClick(object sender, RoutedEventArgs e)
    {
        if (ProfilePicker.SelectedItem is not PomodoroProfile profile) { PomoMessage.Text = "Elige un perfil guardado."; return; }
        var options = profile.Options.Normalize();
        FocusMinutes.Text = options.FocusMinutes.ToString();
        ShortMinutes.Text = options.ShortBreakMinutes.ToString();
        LongMinutes.Text = options.LongBreakMinutes.ToString();
        RoundCount.Text = options.SessionsBeforeLongBreak.ToString();
        AutoStages.IsChecked = options.AutoAdvance;
        Pomodoro.Configure(options);
        RenderPomodoro();
        SaveSettings();
        PomoMessage.Text = $"Perfil «{profile.Name}» cargado. Pulsa Iniciar.";
    }
    private void DeleteProfileClick(object sender, RoutedEventArgs e)
    {
        if (ProfilePicker.SelectedItem is not PomodoroProfile profile) return;
        _profiles.Remove(profile.Id);
        RefreshProfiles();
        SaveSettings();
        PomoMessage.Text = "Perfil eliminado. El temporizador actual conserva sus etapas.";
    }
}
