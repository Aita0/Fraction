using FractionTrainer.Models;

namespace FractionTrainer.Logic;

public sealed class ExerciseGenerator
{
    private readonly Random _random;
    public ExerciseGenerator(Random? random = null) => _random = random ?? new Random();

    public AssembleExercise CreateAssemble(ShapeKind requested)
    {
        var possible = (from b in Enumerable.Range(2, 9)
                        from m in new[] { 2, 3, 4 }
                        where b * m <= 24
                        select (B: b, Parts: b * m)).ToArray();
        var split = possible[_random.Next(possible.Length)];
        int numerator = _random.Next(1, split.B);
        var shape = ChooseShape(requested, split.Parts, out int rows, out int columns);
        return new AssembleExercise(new Fraction(numerator, split.B), shape, split.Parts, rows, columns);
    }

    public PairExercise CreatePairs(ShapeKind requested)
    {
        var values = PickDistinctValues(3);
        var cards = new List<FractionCard>();
        foreach (var value in values)
        {
            int m1 = _random.Next(1, 3), m2 = _random.Next(m1 + 1, 5);
            cards.Add(CreateCard(value.Numerator * m1, value.Denominator * m1, requested));
            cards.Add(CreateCard(value.Numerator * m2, value.Denominator * m2, requested));
        }
        Shuffle(cards);
        return new PairExercise(cards);
    }

    public ChoiceExercise CreateChoices(ShapeKind requested)
    {
        var target = PickDistinctValues(1)[0];
        int correctCount = _random.Next(1, 4);
        var cards = new List<FractionCard>();
        var multipliers = Enumerable.Range(1, 4).Where(m => target.Denominator * m <= 24).OrderBy(_ => _random.Next()).ToArray();
        if (multipliers.Length < correctCount) return CreateChoices(requested);
        // Первый эквивалент обязательно имеет иное число частей.
        if (correctCount > 0 && multipliers[0] == 1) (multipliers[0], multipliers[^1]) = (multipliers[^1], multipliers[0]);
        for (int i = 0; i < correctCount; i++) cards.Add(CreateCard(target.Numerator * multipliers[i], target.Denominator * multipliers[i], requested));
        while (cards.Count < 4)
        {
            var wrong = PickDistinctValues(1)[0];
            if (FractionMath.AreEqual(wrong, target)) continue;
            int m = _random.Next(1, Math.Max(2, 24 / wrong.Denominator + 1));
            cards.Add(CreateCard(wrong.Numerator * m, wrong.Denominator * m, requested));
        }
        Shuffle(cards);
        return new ChoiceExercise(target, cards);
    }

    public IReadOnlyList<Exercise> CreateSession(ShapeKind kind)
    {
        var exercises = new List<Exercise>();
        exercises.AddRange(Enumerable.Range(0, 4).Select(_ => (Exercise)CreateAssemble(kind)));
        exercises.AddRange(Enumerable.Range(0, 3).Select(_ => (Exercise)CreatePairs(kind)));
        var choices = Enumerable.Range(0, 3).Select(_ => CreateChoices(kind)).ToList();
        // В каждой сессии хотя бы одно задание требует выбрать несколько карточек.
        while (choices.All(choice => choice.Cards.Count(card => FractionMath.AreEqual(card.Fraction, choice.Target)) == 1))
            choices[0] = CreateChoices(kind);
        exercises.AddRange(choices);
        Shuffle(exercises);
        return exercises;
    }

    private FractionCard CreateCard(int numerator, int denominator, ShapeKind requested)
    {
        var shape = ChooseShape(requested, denominator, out int rows, out int columns);
        return new FractionCard(new Fraction(numerator, denominator), shape, rows, columns);
    }

    private ShapeKind ChooseShape(ShapeKind requested, int parts, out int rows, out int columns)
    {
        rows = 1; columns = parts;
        var gridSizes = Enumerable.Range(2, 3).SelectMany(r => Enumerable.Range(2, 7).Select(c => (r, c))).Where(x => x.r * x.c == parts).ToArray();
        var allowed = new List<ShapeKind> { ShapeKind.Circle, ShapeKind.Strips };
        if (gridSizes.Length > 0) allowed.Add(ShapeKind.Grid);
        var shape = requested == ShapeKind.Random || !allowed.Contains(requested) ? allowed[_random.Next(allowed.Count)] : requested;
        if (shape == ShapeKind.Grid) (rows, columns) = gridSizes[_random.Next(gridSizes.Length)];
        return shape;
    }

    private List<Fraction> PickDistinctValues(int count)
    {
        var pool = new[] { new Fraction(1, 2), new Fraction(1, 3), new Fraction(2, 3), new Fraction(1, 4), new Fraction(3, 4), new Fraction(2, 5), new Fraction(3, 5), new Fraction(4, 5) }.OrderBy(_ => _random.Next()).ToList();
        return pool.Take(count).ToList();
    }

    private void Shuffle<T>(IList<T> items)
    {
        for (int i = items.Count - 1; i > 0; i--) { int j = _random.Next(i + 1); (items[i], items[j]) = (items[j], items[i]); }
    }
}
