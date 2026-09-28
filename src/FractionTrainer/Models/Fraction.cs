namespace FractionTrainer.Models;

public sealed record Fraction
{
    public int Numerator { get; }
    public int Denominator { get; }

    public Fraction(int numerator, int denominator)
    {
        if (denominator < 2) throw new ArgumentOutOfRangeException(nameof(denominator));
        if (numerator < 0 || numerator > denominator) throw new ArgumentOutOfRangeException(nameof(numerator));
        Numerator = numerator;
        Denominator = denominator;
    }

    public override string ToString() => $"{Numerator}/{Denominator}";
}
