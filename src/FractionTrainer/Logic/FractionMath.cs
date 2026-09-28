using FractionTrainer.Models;

namespace FractionTrainer.Logic;

public static class FractionMath
{
    public static bool AreEqual(long firstNumerator, long firstDenominator, long secondNumerator, long secondDenominator)
    {
        if (firstDenominator <= 0 || secondDenominator <= 0) throw new ArgumentOutOfRangeException(nameof(firstDenominator));
        return firstNumerator * secondDenominator == secondNumerator * firstDenominator;
    }

    public static bool AreEqual(Fraction first, Fraction second) =>
        AreEqual(first.Numerator, first.Denominator, second.Numerator, second.Denominator);

    public static bool IsAssembledCorrectly(AssembleExercise exercise, int selectedCount) =>
        AreEqual(selectedCount, exercise.PartCount, exercise.Target.Numerator, exercise.Target.Denominator);

    public static bool ArePairsCorrect(PairExercise exercise, IReadOnlyList<(int First, int Second)> pairs)
    {
        if (pairs.Count != 3 || pairs.SelectMany(pair => new[] { pair.First, pair.Second }).Distinct().Count() != 6) return false;
        return pairs.All(pair => pair.First >= 0 && pair.Second >= 0 && pair.First < exercise.Cards.Count && pair.Second < exercise.Cards.Count
            && AreEqual(exercise.Cards[pair.First].Fraction, exercise.Cards[pair.Second].Fraction));
    }

    public static bool AreChoicesCorrect(ChoiceExercise exercise, IEnumerable<int> selectedIndices)
    {
        var selected = selectedIndices.ToHashSet();
        var expected = exercise.Cards.Select((card, index) => (card, index))
            .Where(item => AreEqual(item.card.Fraction, exercise.Target)).Select(item => item.index).ToHashSet();
        return selected.SetEquals(expected);
    }
}
