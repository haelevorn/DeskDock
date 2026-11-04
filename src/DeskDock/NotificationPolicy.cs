using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DeskDock;

internal static class NotificationPolicy
{
    // Solo el contador del título. No se consulta DOM, texto de chats ni remitentes.
    public static int? UnreadCount(string title)
    {
        title = Regex.Replace(title, @"[\u200e\u200f\u202a-\u202e\u2066-\u2069]", "").Trim();
        var match = Regex.Match(title, @"^(?:\((?<count>[0-9]{1,7})\)\s*WhatsApp(?:\s+Web)?|WhatsApp(?:\s+Web)?\s*\((?<count>[0-9]{1,7})\))$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups["count"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int count)) return count;
        return title.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase) || title.Equals("WhatsApp Web", StringComparison.OrdinalIgnoreCase) ? 0 : null;
    }
}

internal enum WhatsAppNoticeSource { WebEvent, UnreadCounter }

internal sealed class NotificationDeduplicator
{
    private WhatsAppNoticeSource? _lastSource;
    private TimeSpan _lastTime;
    public bool ShouldPresent(WhatsAppNoticeSource source, TimeSpan now)
    {
        // Agrupar solo el par contador/evento del mismo instante, nunca dos eventos web seguidos.
        if (_lastSource.HasValue && _lastSource != source && now >= _lastTime && now - _lastTime < TimeSpan.FromSeconds(2))
        {
            _lastSource = null;
            return false;
        }
        _lastSource = source;
        _lastTime = now;
        return true;
    }
}
