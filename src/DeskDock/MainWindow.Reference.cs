using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DeskDock;

public partial class MainWindow
{
    private string? _referencePath;
    private bool _referenceFitted = true, _referenceDragging;
    private Point _referencePointer;
    private Matrix _referenceMatrix = Matrix.Identity;

    private void OpenReferenceClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Imagen de referencia", Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true) LoadReference(dialog.FileName);
    }

    internal bool LoadReference(string path, bool save = true)
    {
        try
        {
            var bitmap = ReferenceImageLoader.Load(path);
            ReferencePhoto.Source = bitmap;
            ReferencePhoto.Width = bitmap.PixelWidth;
            ReferencePhoto.Height = bitmap.PixelHeight;
            _referencePath = Path.GetFullPath(path);
            ReferenceHint.Visibility = Visibility.Collapsed;
            _referenceFitted = true;
            FitReference();
            if (save) SaveSettings();
            return true;
        }
        catch (Exception ex)
        {
            Diagnostics.Record("ReferenceImage", ex);
            ReferenceStatus.Text = "No se pudo abrir la imagen. Usa PNG/JPG/BMP/GIF/TIFF de hasta 50 MB y 100 megapíxeles.";
            return false;
        }
    }

    private void ClearReferenceClick(object sender, RoutedEventArgs e)
    {
        ReferencePhoto.Source = null;
        _referencePath = null;
        ReferenceHint.Visibility = Visibility.Visible;
        ReferenceStatus.Text = "Abre o arrastra una imagen local.";
        SaveSettings();
    }

    private void FitReferenceClick(object sender, RoutedEventArgs e) { _referenceFitted = true; FitReference(); }
    private void ReferenceSizeChanged(object sender, SizeChangedEventArgs e) { if (_referenceFitted) FitReference(); }
    private void FitReference()
    {
        if (ReferencePhoto.Source is not BitmapSource bitmap || ReferenceCanvas.ActualWidth < 1 || ReferenceCanvas.ActualHeight < 1) return;
        double scale = Math.Min(ReferenceCanvas.ActualWidth / bitmap.PixelWidth, ReferenceCanvas.ActualHeight / bitmap.PixelHeight);
        _referenceMatrix = new Matrix(scale, 0, 0, scale, (ReferenceCanvas.ActualWidth - bitmap.PixelWidth * scale) / 2,
            (ReferenceCanvas.ActualHeight - bitmap.PixelHeight * scale) / 2);
        PresentReferenceTransform();
    }

    private void PresentReferenceTransform()
    {
        ReferencePhoto.RenderTransform = new MatrixTransform(_referenceMatrix);
        ReferenceStatus.Text = $"{Path.GetFileName(_referencePath)} · {_referenceMatrix.M11 * 100:0}%";
    }

    private void ZoomReference(double factor, Point anchor)
    {
        if (ReferencePhoto.Source is null) return;
        double current = _referenceMatrix.M11;
        double next = Math.Clamp(current * factor, 0.01, 12);
        double ratio = next / current;
        _referenceMatrix = new Matrix(next, 0, 0, next,
            anchor.X - (anchor.X - _referenceMatrix.OffsetX) * ratio,
            anchor.Y - (anchor.Y - _referenceMatrix.OffsetY) * ratio);
        _referenceFitted = false;
        PresentReferenceTransform();
    }
    private void ZoomInReferenceClick(object sender, RoutedEventArgs e) => ZoomReference(1.25, new(ReferenceCanvas.ActualWidth / 2, ReferenceCanvas.ActualHeight / 2));
    private void ZoomOutReferenceClick(object sender, RoutedEventArgs e) => ZoomReference(0.8, new(ReferenceCanvas.ActualWidth / 2, ReferenceCanvas.ActualHeight / 2));
    private void ReferenceWheel(object sender, MouseWheelEventArgs e) { ZoomReference(e.Delta > 0 ? 1.15 : 1 / 1.15, e.GetPosition(ReferenceCanvas)); e.Handled = true; }
    private void ReferenceDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferencePhoto.Source is null) return;
        _referenceDragging = true;
        _referencePointer = e.GetPosition(ReferenceCanvas);
        ReferenceCanvas.CaptureMouse();
        ReferenceCanvas.Cursor = Cursors.Hand;
    }
    private void ReferenceMove(object sender, MouseEventArgs e)
    {
        if (!_referenceDragging) return;
        Point point = e.GetPosition(ReferenceCanvas);
        _referenceMatrix.OffsetX += point.X - _referencePointer.X;
        _referenceMatrix.OffsetY += point.Y - _referencePointer.Y;
        _referencePointer = point;
        _referenceFitted = false;
        PresentReferenceTransform();
    }
    private void ReferenceUp(object sender, MouseButtonEventArgs e) { ReferenceCanvas.ReleaseMouseCapture(); _referenceDragging = false; }
    private void ReferenceLostCapture(object sender, MouseEventArgs e) { _referenceDragging = false; ReferenceCanvas.Cursor = Cursors.Arrow; }
    private void ReferenceDragOver(object sender, DragEventArgs e) { e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; e.Handled = true; }
    private void ReferenceDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } paths) LoadReference(paths[0]);
        e.Handled = true;
    }
}
