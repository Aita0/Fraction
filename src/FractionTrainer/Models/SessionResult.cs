namespace FractionTrainer.Models;

public sealed record SessionResult(DateTime CompletedAt, int CorrectAnswers, int TotalQuestions, TimeSpan Duration)
{
    public int Percent => TotalQuestions == 0 ? 0 : (int)Math.Round(CorrectAnswers * 100d / TotalQuestions);
}
