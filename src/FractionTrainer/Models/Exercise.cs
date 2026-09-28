namespace FractionTrainer.Models;

public enum ShapeKind { Random, Circle, Strips, Grid }
public enum ExerciseType { Assemble, FindPairs, SelectAll }

public abstract record Exercise(ExerciseType Type);

public sealed record AssembleExercise(Fraction Target, ShapeKind ShapeKind, int PartCount, int GridRows = 1, int GridColumns = 1)
    : Exercise(ExerciseType.Assemble);

public sealed record FractionCard(Fraction Fraction, ShapeKind ShapeKind, int GridRows = 1, int GridColumns = 1)
{
    public int PartCount => GridRows * GridColumns;
}

public sealed record PairExercise(IReadOnlyList<FractionCard> Cards) : Exercise(ExerciseType.FindPairs);

public sealed record ChoiceExercise(Fraction Target, IReadOnlyList<FractionCard> Cards) : Exercise(ExerciseType.SelectAll);
