using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DeskDock;

internal static class PomodoroStagesSelfTests
{
    internal static void Run(List<string> results)
    {
        TimeSpan now = TimeSpan.Zero;
        var timer = new PomodoroTimer(() => now);
        timer.Configure(new(1, 1, 1, 12, true));
        timer.Start();
        now += TimeSpan.FromMinutes(1);
        timer.Tick();
        timer.NextStage();
        Check(timer.CycleResults.SequenceEqual(new[] { PomodoroStageResult.Completed, PomodoroStageResult.Skipped }) && timer.CycleStageIndex == 2,
            "Etapas distinguen fin por tiempo y salto manual", results);
        var full = new PomodoroStagesView();
        var compact = new PomodoroStagesView { Compact = true };
        full.SetTimer(timer); compact.SetTimer(timer);
        Check(full.VisibleStageIndices.Count == 24 && compact.VisibleStageIndices.Count <= 7 && compact.VisibleStageIndices.Contains(2),
            "Ciclo de 12 sesiones conserva 24 etapas completas y solo cinco cercanas en compacto", results);
        Check(((string)full.ToolTip).Contains("Terminada") && ((string)full.ToolTip).Contains("Saltada") && ((string)full.ToolTip).Contains("Descanso largo 12/12"),
            "Descripción accesible explica formas, estado y todas las etapas", results);
        while (timer.CycleStageIndex < 12) timer.NextStage();
        compact.SetTimer(timer);
        Check(compact.VisibleStageIndices[0] is null && compact.VisibleStageIndices[^1] is null && compact.VisibleStageIndices.Contains(12),
            "Ventana de etapas sigue la actual con puntos a ambos extremos", results);
        while (timer.Stage != PomodoroStage.LongBreak) timer.NextStage();
        timer.NextStage();
        Check(timer.CycleStageIndex == 0 && timer.CycleResults.Count == 0 && timer.CompletedFocusSessions == 12,
            "Tras descanso largo comienza un ciclo visual limpio conservando sesiones totales", results);
        timer.NextStage(); timer.Configure(new());
        Check(timer.CycleResults.Count == 0 && timer.CycleSession == 1, "Reconfigurar o reiniciar limpia las marcas anteriores", results);
        for (int rounds = 2; rounds <= 12; rounds++)
        {
            timer.Configure(new(1, 1, 1, rounds));
            full.SetTimer(timer); compact.SetTimer(timer);
            full.Measure(new Size(240, double.PositiveInfinity));
            compact.Measure(new Size(100, 20));
            Check(full.DesiredSize.Width <= 216 && full.DesiredSize.Height <= 44 && compact.DesiredSize.Width <= 96 && compact.DesiredSize.Height == 14,
                $"{rounds} sesiones caben en dos filas grandes o una tira compacta fija", results);
        }
        int revision = timer.Revision;
        timer.Start(); now += TimeSpan.FromSeconds(1); timer.Tick();
        Check(timer.Revision == revision, "El reloj no reconstruye la tira de etapas en cada actualización", results);
        timer.Reset(); timer.Start(); now += TimeSpan.FromMinutes(1); timer.NextStage();
        Check(timer.CycleResults[0] == PomodoroStageResult.Completed, "Saltar justo al vencer marca la etapa como terminada", results);
    }

    private static void Check(bool condition, string message, List<string> results)
    {
        if (!condition) throw new InvalidOperationException(message);
        results.Add("PASS: " + message);
    }
}
