namespace FractionTrainer.Drawing;

public sealed class GridRenderer(int rows, int columns) : IShapeRenderer
{
    public int Rows { get; } = rows;
    public int Columns { get; } = columns;
    public int PartCount => Rows * Columns;

    private RectangleF Figure(Rectangle bounds)
    {
        var area = ShapeGeometry.FitSquare(bounds);
        float cell = Math.Min(area.Width / Columns, area.Height / Rows);
        float width = cell * Columns, height = cell * Rows;
        return new RectangleF(area.Left + (area.Width - width) / 2, area.Top + (area.Height - height) / 2, width, height);
    }

    public void Draw(Graphics graphics, Rectangle bounds, IReadOnlyList<bool> selected, Color selectedColor)
    {
        var figure = Figure(bounds);
        float w = figure.Width / Columns, h = figure.Height / Rows;
        using var empty = new SolidBrush(Color.FromArgb(235, 238, 241));
        using var filled = new SolidBrush(selectedColor);
        using var pen = new Pen(Color.White, 3f);
        for (int row = 0; row < Rows; row++)
        for (int col = 0; col < Columns; col++)
        {
            int index = row * Columns + col;
            var cell = new RectangleF(figure.Left + col * w, figure.Top + row * h, w, h);
            graphics.FillRectangle(selected[index] ? filled : empty, cell);
            graphics.DrawRectangle(pen, cell.X, cell.Y, cell.Width, cell.Height);
        }
        using var outline = new Pen(Color.SlateGray, 1.5f);
        graphics.DrawRectangle(outline, figure.X, figure.Y, figure.Width, figure.Height);
    }

    public int HitTest(Point point, Rectangle bounds)
    {
        var figure = Figure(bounds);
        if (!figure.Contains(point)) return -1;
        int col = Math.Min(Columns - 1, (int)((point.X - figure.Left) / (figure.Width / Columns)));
        int row = Math.Min(Rows - 1, (int)((point.Y - figure.Top) / (figure.Height / Rows)));
        return row * Columns + col;
    }
}
