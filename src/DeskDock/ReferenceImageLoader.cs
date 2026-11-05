using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeskDock;

internal static class ReferenceImageLoader
{
    internal static BitmapSource Grayscale(BitmapSource original)
    {
        // Mantener alfa, dimensiones y DPI; nunca escribir en la imagen ni en el archivo original.
        var color = new FormatConvertedBitmap(original, PixelFormats.Bgra32, null, 0);
        int stride = checked(color.PixelWidth * 4);
        var pixels = new byte[checked(stride * color.PixelHeight)];
        color.CopyPixels(pixels, stride, 0);
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte value = (byte)((54 * pixels[i + 2] + 183 * pixels[i + 1] + 19 * pixels[i] + 128) >> 8);
            pixels[i] = pixels[i + 1] = pixels[i + 2] = value;
        }
        var gray = BitmapSource.Create(color.PixelWidth, color.PixelHeight, original.DpiX, original.DpiY, PixelFormats.Bgra32, null, pixels, stride);
        gray.Freeze();
        return gray;
    }

    internal static (int Width, int Height) Inspect(string path)
    {
        if (new FileInfo(path).Length > 50_000_000) throw new InvalidDataException("La imagen debe ocupar menos de 50 MB.");
        using var stream = File.OpenRead(path);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnDemand);
        var frame = decoder.Frames[0];
        int width = frame.PixelWidth, height = frame.PixelHeight;
        if (width <= 0 || height <= 0 || (long)width * height > 100_000_000)
            throw new InvalidDataException("La imagen supera el límite de 100 megapíxeles.");
        return (width, height);
    }

    public static BitmapSource Load(string path)
    {
        var (width, height) = Inspect(path);
        using var stream = File.OpenRead(path);
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
