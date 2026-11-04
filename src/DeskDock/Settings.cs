using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace DeskDock;

internal sealed record Settings(bool AlwaysOnTop = false, bool MinimizeToTray = true, string Skin = "minimal", bool NotificationSound = true,
    double WindowWidth = 360, double WindowHeight = 520, PomodoroOptions? Pomodoro = null,
    List<PomodoroProfile>? Profiles = null, string? ReferenceImagePath = null)
{
    private static string FilePath => Path.Combine(Diagnostics.Root, "settings.json");
    public static Settings Load()
    {
        try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Diagnostics.Root);
            File.WriteAllText(FilePath + ".tmp", JsonSerializer.Serialize(this));
            File.Move(FilePath + ".tmp", FilePath, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { Diagnostics.Record("Settings", ex); }
    }
}
