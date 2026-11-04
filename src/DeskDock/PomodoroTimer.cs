using System;
using System.Diagnostics;

namespace DeskDock;

internal enum PomodoroStage { Focus, ShortBreak, LongBreak }
internal sealed record PomodoroOptions(int FocusMinutes = 25, int ShortBreakMinutes = 5,
    int LongBreakMinutes = 15, int SessionsBeforeLongBreak = 4, bool AutoAdvance = true)
{
    public PomodoroOptions Normalize() => new(Math.Clamp(FocusMinutes, 1, 240), Math.Clamp(ShortBreakMinutes, 1, 60),
        Math.Clamp(LongBreakMinutes, 1, 120), Math.Clamp(SessionsBeforeLongBreak, 2, 12), AutoAdvance);
}

internal sealed class PomodoroTimer
{
    private readonly Func<TimeSpan> _now;
    private TimeSpan _deadline;
    public PomodoroOptions Options { get; private set; } = new();
    public PomodoroStage Stage { get; private set; }
    public int CompletedFocusSessions { get; private set; }
    public bool IsRunning { get; private set; }
    public TimeSpan Remaining { get; private set; } = TimeSpan.FromMinutes(25);
    public TimeSpan Duration => Stage switch
    {
        PomodoroStage.ShortBreak => TimeSpan.FromMinutes(Options.ShortBreakMinutes),
        PomodoroStage.LongBreak => TimeSpan.FromMinutes(Options.LongBreakMinutes),
        _ => TimeSpan.FromMinutes(Options.FocusMinutes)
    };

    public PomodoroTimer(Func<TimeSpan>? now = null) =>
        _now = now ?? (() => TimeSpan.FromSeconds((double)Stopwatch.GetTimestamp() / Stopwatch.Frequency));

    public void Configure(PomodoroOptions options) { Options = options.Normalize(); Reset(); }
    public void Reset() { IsRunning = false; Stage = PomodoroStage.Focus; CompletedFocusSessions = 0; Remaining = Duration; }
    public void Start() { if (IsRunning) return; _deadline = _now() + Remaining; IsRunning = true; }
    public void Pause() { if (!IsRunning) return; Remaining = MaxZero(_deadline - _now()); IsRunning = false; }

    // Un vencimiento por comprobación. Tras suspensión no simula etapas que nadie realizó.
    public PomodoroStage? Tick()
    {
        if (!IsRunning) return null;
        var now = _now();
        Remaining = MaxZero(_deadline - now);
        if (Remaining > TimeSpan.Zero) return null;
        var finished = Stage;
        if (Stage == PomodoroStage.Focus)
        {
            CompletedFocusSessions++;
            Stage = CompletedFocusSessions % Options.SessionsBeforeLongBreak == 0 ? PomodoroStage.LongBreak : PomodoroStage.ShortBreak;
        }
        else Stage = PomodoroStage.Focus;
        Remaining = Duration;
        IsRunning = Options.AutoAdvance;
        _deadline = now + Remaining;
        return finished;
    }

    public static string StageName(PomodoroStage stage) => stage switch
    { PomodoroStage.ShortBreak => "Descanso corto", PomodoroStage.LongBreak => "Descanso largo", _ => "Concentración" };
    private static TimeSpan MaxZero(TimeSpan value) => value < TimeSpan.Zero ? TimeSpan.Zero : value;
}
