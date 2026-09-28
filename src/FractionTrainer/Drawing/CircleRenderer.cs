using System.Drawing.Drawing2D;

namespace FractionTrainer.Drawing;

public sealed class CircleRenderer(int partCount) : IShapeRenderer
{
    public int PartCount { get; } = partCount;

    public void Draw(Graphics graphics, Rectangle bounds, IReadOnlyList<bool> selected, Color selectedColor)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var circle = ShapeGeometry.FitSquare(bounds);
        float sweep = 360f / PartCount;
        using var empty = new SolidBrush(Color.FromArgb(235, 238, 241));
        using var filled = new SolidBrush(selectedColor);
        using var border = new Pen(Color.White, 3f);
        for (int i = 0; i < PartCount; i++)
        {
            // Одинаковый угол sweep гарантирует равную площадь секторов.
            graphics.FillPie(selected[i] ? filled : empty, circle, -90 + i * sweep, sweep);
            graphics.DrawPie(border, circle, -90 + i * sweep, sweep);
        }
        using var outline = new Pen(Color.SlateGray, 1.5f);
        graphics.DrawEllipse(outline, circle);
    }

    public int HitTest(Point point, Rectangle bounds)
    {
        var circle = ShapeGeometry.FitSquare(bounds);
        double cx = circle.Left + circle.Width / 2, cy = circle.Top + circle.Height / 2;
        double dx = point.X - cx, dy = point.Y - cy;
        double radius = circle.Width / 2;
        if (dx * dx + dy * dy > radius * radius) return -1;
        // atan2 даёт единственный сектор даже для точки на общей границе.
        double angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 450) % 360;
        return Math.Min(PartCount - 1, (int)(angle / (360d / PartCount)));
    }
}
