using FractionTrainer.Drawing;
using FractionTrainer.Models;

namespace FractionTrainer.UI;

public sealed class FractionCardControl : Panel
{
    private readonly Label _badge;
    public FractionCard Card { get; }
    public int CardIndex { get; }
    public bool IsChosen { get; private set; }

    public FractionCardControl(FractionCard card, int index, Color color)
    {
        Card = card; CardIndex = index; Width = 220; Height = 210; Margin = new Padding(10); Padding = new Padding(5);
        BorderStyle = BorderStyle.FixedSingle; Cursor = Cursors.Hand; BackColor = Color.White;
        var canvas = new ShapeCanvas { Dock = DockStyle.Fill, InteractionEnabled = false, SelectedColor = color };
        canvas.SetRenderer(CreateRenderer(card)); canvas.SetSelectedCount(card.Fraction.Numerator); canvas.InteractionEnabled = false;
        var label = new Label { Text = card.Fraction.ToString(), Dock = DockStyle.Bottom, Height = 32, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        _badge = new Label { Dock = DockStyle.Top, Height = 27, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        Controls.Add(canvas); Controls.Add(label); Controls.Add(_badge);
        foreach (Control control in Controls) control.Click += ForwardClick;
    }

    public void SetChosen(bool chosen)
    {
        IsChosen = chosen; BackColor = chosen ? Color.FromArgb(220, 244, 255) : Color.White;
        Padding = new Padding(chosen ? 5 : 3); Invalidate();
    }

    public void SetPair(int? number, Color color)
    {
        _badge.Text = number is null ? "" : $"Пара {number}";
        _badge.ForeColor = color; SetChosen(number is not null);
    }

    private void ForwardClick(object? sender, EventArgs e) => OnClick(e);

    private static IShapeRenderer CreateRenderer(FractionCard card) => card.ShapeKind switch
    {
        ShapeKind.Circle => new CircleRenderer(card.PartCount),
        ShapeKind.Strips => new StripsRenderer(card.PartCount),
        ShapeKind.Grid => new GridRenderer(card.GridRows, card.GridColumns),
        _ => throw new InvalidOperationException()
    };
}
