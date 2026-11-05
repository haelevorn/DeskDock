using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DeskDock;

internal static class FocusSelfTests
{
    public static void Run(List<string> results)
    {
        TimeSpan now = TimeSpan.Zero;
        var timer = new PomodoroTimer(() => now);
        timer.Configure(new(1, 1, 2, 2, true));
        timer.Start();
        now += TimeSpan.FromSeconds(15);
        Check(timer.Tick() is null && timer.Remaining == TimeSpan.FromSeconds(45), "Cuenta regresiva por tiempo transcurrido", results);
        timer.Pause();
        now += TimeSpan.FromHours(1);
        Check(timer.Tick() is null && timer.Remaining == TimeSpan.FromSeconds(45), "Pausa conserva tiempo aunque pasen minutos", results);
        timer.Start();
        now += TimeSpan.FromSeconds(45);
        Check(timer.Tick() == PomodoroStage.Focus && timer.Stage == PomodoroStage.ShortBreak && timer.CompletedFocusSessions == 1,
            "Fin de concentración inicia descanso corto una sola vez", results);
        Check(timer.Tick() is null, "No repetir aviso en el mismo instante", results);
        now += TimeSpan.FromMinutes(1);
        Check(timer.Tick() == PomodoroStage.ShortBreak && timer.Stage == PomodoroStage.Focus, "Descanso corto vuelve a concentración", results);
        now += TimeSpan.FromMinutes(1);
        Check(timer.Tick() == PomodoroStage.Focus && timer.Stage == PomodoroStage.LongBreak && timer.CompletedFocusSessions == 2,
            "Descanso largo tras las sesiones configuradas", results);
        now += TimeSpan.FromMinutes(2);
        Check(timer.Tick() == PomodoroStage.LongBreak && timer.Stage == PomodoroStage.Focus, "Nuevo ciclo después del descanso largo", results);
        now += TimeSpan.FromHours(3);
        Check(timer.Tick() == PomodoroStage.Focus && timer.CompletedFocusSessions == 3 && timer.Stage == PomodoroStage.ShortBreak,
            "Comprobación tardía no simula sesiones no realizadas", results);
        timer.Reset();
        Check(!timer.IsRunning && timer.CompletedFocusSessions == 0 && timer.Remaining == TimeSpan.FromMinutes(1), "Reinicio completo y detenido", results);
        timer.Configure(new(1, 1, 2, 2, false));
        timer.Start();
        now += TimeSpan.FromMinutes(1);
        Check(timer.Tick() == PomodoroStage.Focus && !timer.IsRunning && timer.Stage == PomodoroStage.ShortBreak,
            "Avance manual espera iniciar la siguiente etapa", results);
        Check(new PomodoroOptions(0, -5, 999, 1).Normalize() == new PomodoroOptions(1, 1, 120, 2), "Límites de duraciones guardadas", results);
        TimeSpan skipNow = TimeSpan.Zero;
        var skip = new PomodoroTimer(() => skipNow);
        skip.Configure(new(1, 1, 2, 2, false));
        skip.Start();
        skipNow += TimeSpan.FromSeconds(10);
        Check(skip.NextStage() == PomodoroStage.Focus && skip.Stage == PomodoroStage.ShortBreak && skip.IsRunning &&
              skip.Remaining == TimeSpan.FromMinutes(1) && skip.CompletedFocusSessions == 1,
            "Siguiente etapa inicia descanso completo aunque el avance automático esté desactivado", results);
        skipNow += TimeSpan.FromSeconds(5);
        Check(skip.Tick() is null && skip.Remaining == TimeSpan.FromSeconds(55), "Salto manual renueva el plazo de la siguiente etapa", results);
        Check(skip.NextStage() == PomodoroStage.ShortBreak && skip.Stage == PomodoroStage.Focus && skip.CompletedFocusSessions == 1,
            "Saltar descanso inicia concentración sin sumar otra sesión", results);
        skip.Pause();
        Check(skip.NextStage() == PomodoroStage.Focus && skip.Stage == PomodoroStage.LongBreak && skip.IsRunning &&
              skip.Remaining == TimeSpan.FromMinutes(2) && skip.CompletedFocusSessions == 2,
            "Saltar concentración pausada respeta el descanso largo programado", results);
        Check(skip.NextStage() == PomodoroStage.LongBreak && skip.Stage == PomodoroStage.Focus && skip.Remaining == TimeSpan.FromMinutes(1),
            "Saltar descanso largo vuelve a concentración con tiempo completo", results);
        skip.Reset();
        skip.Start();
        skipNow += TimeSpan.FromMinutes(1);
        skip.NextStage();
        Check(skip.CompletedFocusSessions == 1 && skip.Stage == PomodoroStage.ShortBreak && skip.Tick() is null,
            "Salto en el instante de vencimiento avanza una sola etapa", results);
        Check(!MainWindow.ReadMinutes("0", 1, 240, out _) && !MainWindow.ReadMinutes("1.5", 1, 240, out _) &&
              MainWindow.ReadMinutes("90", 1, 240, out _), "Validación de minutos enteros sin valores inválidos", results);
        Check(MainWindow.SafeDimension(double.NaN, 360, 300, 1920) == 360 && MainWindow.SafeDimension(10, 360, 300, 1920) == 300 &&
              MainWindow.SafeDimension(5000, 360, 300, 1920) == 1920, "Tamaño guardado válido y dentro de pantalla", results);
        var profiles = new PomodoroProfiles();
        var original = profiles.Upsert("Dibujo", new(45, 8, 20, 3, false));
        var updated = profiles.Upsert(" dibujo ", new(50, 10, 25, 3, true));
        Check(profiles.Items.Count == 1 && original.Id == updated.Id && updated.Options.FocusMinutes == 50,
            "Guardar mismo nombre actualiza perfil sin duplicarlo", results);
        profiles.Upsert("Lectura", new(20, 5, 10, 4, false));
        var stored = new Settings(Profiles: profiles.Items.ToList(), ReferenceImagePath: "C:\\imagenes\\referencia.png");
        string legacy = JsonSerializer.Serialize(stored);
        var restored = JsonSerializer.Deserialize<Settings>(legacy[..^1] + ",\"GlassEnabled\":true}")!;
        var restoredProfiles = new PomodoroProfiles(restored.Profiles);
        Check(restoredProfiles.Items.Count == 2 && restoredProfiles.Items[0].Id == updated.Id &&
              restoredProfiles.Items[0].Options.FocusMinutes == 50 && !JsonSerializer.Serialize(restored).Contains("GlassEnabled"),
            "Preferencias antiguas conservan perfiles y descartan Glass", results);
        profiles.Remove(updated.Id);
        Check(profiles.Items.Count == 1 && profiles.Items[0].Name == "Lectura", "Eliminar solo el perfil seleccionado", results);
        bool rejected = false;
        try { profiles.Upsert("  ", new()); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "No guardar perfiles sin nombre", results);
    }

    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
