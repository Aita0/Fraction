namespace FractionTrainer.Drawing;

public interface IShapeRenderer
{
    int PartCount { get; }
    void Draw(Graphics graphics, Rectangle bounds, IReadOnlyList<bool> selected, Color selectedColor);
    int HitTest(Point point, Rectangle bounds);
}
