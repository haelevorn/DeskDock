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

    public static bool IsChatGptNavigation(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort &&
        string.IsNullOrEmpty(uri.UserInfo) && uri.Host.ToLowerInvariant() is
            "chatgpt.com" or "www.chatgpt.com" or "auth.openai.com" or "auth0.openai.com" or "login.openai.com" or
            "accounts.google.com" or "appleid.apple.com";
}
