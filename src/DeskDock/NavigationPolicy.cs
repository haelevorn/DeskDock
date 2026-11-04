using System;

namespace DeskDock;

internal static class NavigationPolicy
{
    public static bool IsWhatsApp(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps && uri.Host.Equals("web.whatsapp.com", StringComparison.OrdinalIgnoreCase) &&
        uri.IsDefaultPort && string.IsNullOrEmpty(uri.UserInfo);

    public static bool IsExternalWeb(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) && string.IsNullOrEmpty(uri.UserInfo);
}
