namespace FractionTrainer.Drawing;

public sealed class StripsRenderer(int partCount) : IShapeRenderer
{
    public int PartCount { get; } = partCount;

    private static RectangleF Figure(Rectangle bounds)
    {
        var square = ShapeGeometry.FitSquare(bounds);
        return new RectangleF(square.Left, square.Top + square.Height * .16f, square.Width, square.Height * .68f);
    }

    public void Draw(Graphics graphics, Rectangle bounds, IReadOnlyList<bool> selected, Color selectedColor)
    {
        var figure = Figure(bounds);
        float width = figure.Width / PartCount;
        using var empty = new SolidBrush(Color.FromArgb(235, 238, 241));
        using var filled = new SolidBrush(selectedColor);
        using var pen = new Pen(Color.White, 3f);
        for (int i = 0; i < PartCount; i++)
        {
            var part = new RectangleF(figure.Left + i * width, figure.Top, width, figure.Height);
            graphics.FillRectangle(selected[i] ? filled : empty, part);
            graphics.DrawRectangle(pen, part.X, part.Y, part.Width, part.Height);
        }
        using var outline = new Pen(Color.SlateGray, 1.5f);
        graphics.DrawRectangle(outline, figure.X, figure.Y, figure.Width, figure.Height);
    }

    public int HitTest(Point point, Rectangle bounds)
    {
        var figure = Figure(bounds);
        if (!figure.Contains(point)) return -1;
        return Math.Min(PartCount - 1, (int)((point.X - figure.Left) / (figure.Width / PartCount)));
    }
}
