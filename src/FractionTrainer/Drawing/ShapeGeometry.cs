namespace FractionTrainer.Drawing;

internal static class ShapeGeometry
{
    public static RectangleF FitSquare(Rectangle bounds)
    {
        float side = Math.Max(1, Math.Min(bounds.Width, bounds.Height) - 30);
        return new RectangleF(bounds.Left + (bounds.Width - side) / 2f, bounds.Top + (bounds.Height - side) / 2f, side, side);
    }
}
