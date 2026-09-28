namespace FractionTrainer.UI;

public sealed class FractionView : Control
{
    public int Numerator { get; set; } = 1;
    public int Denominator { get; set; } = 2;

    public FractionView()
    {
        DoubleBuffered = true;
        MinimumSize = new Size(100, 120);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var font = new Font("Segoe UI", Math.Max(22, Height / 4f), FontStyle.Bold);
        using var pen = new Pen(Color.DeepSkyBlue, 4);
        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        int middle = Height / 2;
        e.Graphics.DrawString(Numerator.ToString(), font, Brushes.DimGray, new Rectangle(0, 0, Width, middle - 5), format);
        e.Graphics.DrawLine(pen, Width * .3f, middle, Width * .7f, middle);
        e.Graphics.DrawString(Denominator.ToString(), font, Brushes.DimGray, new Rectangle(0, middle + 5, Width, Height - middle - 5), format);
    }
}
