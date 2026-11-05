using System;
using System.Windows;
using System.Windows.Media;

namespace DeskDock;

internal static class ReferenceTransforms
{
    public static Matrix Fit(Size image, Size viewport, int turns, bool flipped)
    {
        if (image.Width <= 0 || image.Height <= 0 || viewport.Width <= 0 || viewport.Height <= 0) return Matrix.Identity;
        var matrix = Matrix.Identity;
        matrix.Scale(flipped ? -1 : 1, 1);
        matrix.Rotate((turns % 4) * 90);
        var bounds = new MatrixTransform(matrix).TransformBounds(new Rect(new Point(), image));
        matrix.Translate(-bounds.Left, -bounds.Top);
        double scale = Math.Min(viewport.Width / bounds.Width, viewport.Height / bounds.Height);
        matrix.Scale(scale, scale);
        matrix.Translate((viewport.Width - bounds.Width * scale) / 2, (viewport.Height - bounds.Height * scale) / 2);
        return matrix;
    }

    public static double Scale(Matrix matrix) => Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
}
