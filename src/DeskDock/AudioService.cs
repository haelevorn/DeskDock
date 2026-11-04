using NAudio.CoreAudioApi;

namespace DeskDock;

// NAudio es un adaptador de las APIs nativas Core Audio, no un volumen por aplicación.
internal sealed class AudioService
{
    // Se consulta el dispositivo predeterminado en cada operación para seguir cambios de salida.
    public (float Volume, bool Muted, string Device) Read()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return (device.AudioEndpointVolume.MasterVolumeLevelScalar, device.AudioEndpointVolume.Mute, device.FriendlyName);
    }

    public void SetVolume(float value)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = System.Math.Clamp(value, 0, 1);
    }

    public void ToggleMute()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
    }
}
