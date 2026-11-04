using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DeskDock;

// Ambas vistas enlazan con el mismo estado; no dependen del árbol visual de las pestañas.
public sealed class PlayerState : INotifyPropertyChanged
{
    private string _title = "Sin sesión de Spotify", _artist = "Abre Spotify y reproduce música";
    private ImageSource? _artwork;
    private bool _canPlay, _canPause, _canPrevious, _canNext;
    public string Title { get => _title; set => Set(ref _title, value); }
    public string Artist { get => _artist; set => Set(ref _artist, value); }
    public ImageSource? Artwork { get => _artwork; set => Set(ref _artwork, value); }
    public bool CanPlay { get => _canPlay; set => Set(ref _canPlay, value); }
    public bool CanPause { get => _canPause; set => Set(ref _canPause, value); }
    public bool CanPrevious { get => _canPrevious; set => Set(ref _canPrevious, value); }
    public bool CanNext { get => _canNext; set => Set(ref _canNext, value); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
