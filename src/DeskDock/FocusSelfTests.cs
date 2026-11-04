using System;
using System.Collections.Generic;

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
        Check(!MainWindow.ReadMinutes("0", 1, 240, out _) && !MainWindow.ReadMinutes("1.5", 1, 240, out _) &&
              MainWindow.ReadMinutes("90", 1, 240, out _), "Validación de minutos enteros sin valores inválidos", results);
        Check(MainWindow.SafeDimension(double.NaN, 360, 300, 1920) == 360 && MainWindow.SafeDimension(10, 360, 300, 1920) == 300 &&
              MainWindow.SafeDimension(5000, 360, 300, 1920) == 1920, "Tamaño guardado válido y dentro de pantalla", results);
    }

    private static void Check(bool condition, string description, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(description);
        results.Add("PASS: " + description);
    }
}
