using System;
using System.IO;
using System.Media;
using System.Windows;

namespace DeskDock;

internal sealed class NotificationAudio : IDisposable
{
    private Stream? _stream;
    private SoundPlayer? _player;

    public void Play()
    {
        try
        {
            if (_player is null)
            {
                _stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/notification-bwoop.wav"))!.Stream;
                _player = new SoundPlayer(_stream);
                _player.Load();
            }
            _player.Play();
        }
        catch (Exception ex) { Diagnostics.Record("NotificationAudio", ex); }
    }

    public void Dispose()
    {
        _player?.Stop();
        _player?.Dispose();
        _stream?.Dispose();
    }
}
