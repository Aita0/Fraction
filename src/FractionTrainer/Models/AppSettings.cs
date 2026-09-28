using System.Drawing;
using System.Text.Json.Serialization;

namespace FractionTrainer.Models;

public sealed class AppSettings
{
    public ShapeKind ShapeKind { get; set; } = ShapeKind.Random;
    public int SelectedColorArgb { get; set; } = Color.DeepSkyBlue.ToArgb();
    [JsonIgnore]
    public Color SelectedColor => Color.FromArgb(SelectedColorArgb);
}
