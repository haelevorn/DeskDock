using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace DeskDock;

internal enum MediaCommand { Play, Pause, Previous, Next }

internal sealed class SpotifyService
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;

    // Lista explícita: nunca se controla la sesión global de un navegador u otra app.
    public static bool IsSpotifyId(string id) =>
        id.Equals("Spotify.exe", StringComparison.OrdinalIgnoreCase) ||
        id.StartsWith("SpotifyAB.SpotifyMusic_", StringComparison.OrdinalIgnoreCase);

    public async Task<GlobalSystemMediaTransportControlsSession?> FindAsync()
    {
        _manager ??= await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        return _manager.GetSessions().Where(s => IsSpotifyId(s.SourceAppUserModelId))
            .OrderByDescending(s => s.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            .FirstOrDefault();
    }

    public async Task<bool> SendAsync(MediaCommand command)
    {
        // Revalidar justo antes del comando: Spotify puede haberse cerrado o reiniciado.
        var session = await FindAsync();
        if (session is null) return false;
        var controls = session.GetPlaybackInfo().Controls;
        return command switch
        {
            MediaCommand.Play when controls.IsPlayEnabled => await session.TryPlayAsync(),
            MediaCommand.Pause when controls.IsPauseEnabled => await session.TryPauseAsync(),
            MediaCommand.Previous when controls.IsPreviousEnabled => await session.TrySkipPreviousAsync(),
            MediaCommand.Next when controls.IsNextEnabled => await session.TrySkipNextAsync(),
            _ => false
        };
    }
}
