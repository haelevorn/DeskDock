using System;
using System.Windows;

namespace DeskDock;

internal static class SkinManager
{
    public static string Current { get; private set; } = "minimal";
    public static string Normalize(string? id) => id is "aurora" or "eclipse" ? id : "minimal";

    public static void Apply(string? id)
    {
        string selected = Normalize(id);
        string file = selected switch { "aurora" => "Aurora.xaml", "eclipse" => "Eclipse.xaml", _ => "Minimal.xaml" };
        var dictionary = new ResourceDictionary { Source = new Uri("/DeskDock;component/Themes/" + file, UriKind.Relative) };
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        if (dictionaries.Count == 0) dictionaries.Add(dictionary);
        else dictionaries[0] = dictionary;
        Current = selected;
    }
}
