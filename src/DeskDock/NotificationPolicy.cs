using System.Text.RegularExpressions;

namespace DeskDock;

internal static class NotificationPolicy
{
    // Solo el contador del título. No se consulta DOM, texto de chats ni remitentes.
    public static int? UnreadCount(string title)
    {
        var match = Regex.Match(title, @"^\((\d{1,7})\)\s*WhatsApp(?:\s|$)", RegexOptions.CultureInvariant);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int count)) return count;
        return title.Trim() == "WhatsApp" ? 0 : null;
    }
}
