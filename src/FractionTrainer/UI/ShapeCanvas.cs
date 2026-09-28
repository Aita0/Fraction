using FractionTrainer.Drawing;

namespace FractionTrainer.UI;

public sealed class ShapeCanvas : Control
{
    private IShapeRenderer? _renderer;
    private readonly List<bool> _selected = [];
    public event EventHandler? SelectionChanged;
    public Color SelectedColor { get; set; } = Color.DeepSkyBlue;
    public bool InteractionEnabled { get; set; } = true;
    public int SelectedCount => _selected.Count(value => value);

    public ShapeCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.ResizeRedraw, true);
    }

    public void SetRenderer(IShapeRenderer renderer)
    {
        _renderer = renderer;
        _selected.Clear();
        _selected.AddRange(Enumerable.Repeat(false, renderer.PartCount));
        InteractionEnabled = true;
        Invalidate();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearSelection() => SetSelectedCount(0);

    public void SetSelectedCount(int count)
    {
        count = Math.Clamp(count, 0, _selected.Count);
        for (int i = 0; i < _selected.Count; i++) _selected[i] = i < count;
        Invalidate();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _renderer?.Draw(e.Graphics, ClientRectangle, _selected, SelectedColor);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (!InteractionEnabled || _renderer is null) return;
        int index = _renderer.HitTest(e.Location, ClientRectangle);
        if (index < 0) return;
        _selected[index] = !_selected[index];
        Invalidate();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
