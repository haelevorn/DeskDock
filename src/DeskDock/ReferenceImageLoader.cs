using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace DeskDock;

internal static class ReferenceImageLoader
{
    public static BitmapSource Load(string path)
    {
        if (new FileInfo(path).Length > 50_000_000) throw new InvalidDataException("La imagen debe ocupar menos de 50 MB.");
        using var stream = File.OpenRead(path);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand);
        var frame = decoder.Frames[0];
        int width = frame.PixelWidth, height = frame.PixelHeight;
        if (width <= 0 || height <= 0 || (long)width * height > 100_000_000)
            throw new InvalidDataException("La imagen supera el límite de 100 megapíxeles.");
        stream.Position = 0;
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        if (width >= height) bitmap.DecodePixelWidth = Math.Min(width, 4096);
        else bitmap.DecodePixelHeight = Math.Min(height, 4096);
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
