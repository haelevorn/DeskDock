using System;
using System.Collections.Generic;
using System.Linq;

namespace DeskDock;

public sealed record PomodoroProfile(Guid Id, string Name, PomodoroOptions Options);

internal sealed class PomodoroProfiles
{
    private readonly List<PomodoroProfile> _items = new();
    public IReadOnlyList<PomodoroProfile> Items => _items;
    public PomodoroProfiles(IEnumerable<PomodoroProfile>? items = null)
    {
        if (items is null) return;
        foreach (var item in items.Take(50))
        {
            if (item is null || item.Options is null || string.IsNullOrWhiteSpace(item.Name)) continue;
            string name = item.Name.Trim();
            if (name.Length > 40) name = name[..40];
            if (name.Any(char.IsControl)) continue;
            int index = _items.FindIndex(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            var profile = new PomodoroProfile(item.Id == Guid.Empty ? Guid.NewGuid() : item.Id, name, item.Options.Normalize());
            if (index >= 0) _items[index] = profile;
            else _items.Add(profile);
        }
    }

    public PomodoroProfile Upsert(string name, PomodoroOptions options)
    {
        name = name.Trim();
        if (name.Length is < 1 or > 40 || name.Any(char.IsControl))
            throw new ArgumentException("Escribe un nombre de 1 a 40 caracteres.");
        int index = _items.FindIndex(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index < 0 && _items.Count >= 50) throw new ArgumentException("Puedes guardar hasta 50 perfiles.");
        var profile = new PomodoroProfile(index >= 0 ? _items[index].Id : Guid.NewGuid(), name, options.Normalize());
        if (index >= 0) _items[index] = profile;
        else _items.Add(profile);
        return profile;
    }

    public void Remove(Guid id) => _items.RemoveAll(p => p.Id == id);
}
