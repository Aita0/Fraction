using FractionTrainer.Models;

namespace FractionTrainer.Logic;

public sealed class KnowledgeSession
{
    private readonly IReadOnlyList<Exercise> _exercises;
    private readonly DateTime _startedAt;
    private bool _currentAnswered;

    public KnowledgeSession(IReadOnlyList<Exercise> exercises, DateTime? startedAt = null)
    {
        if (exercises.Count == 0) throw new ArgumentException("Нужен хотя бы один вопрос.", nameof(exercises));
        _exercises = exercises;
        _startedAt = startedAt ?? DateTime.Now;
    }

    public int CurrentIndex { get; private set; }
    public int CorrectAnswers { get; private set; }
    public int TotalQuestions => _exercises.Count;
    public bool IsCurrentAnswered => _currentAnswered;
    public bool IsFinished => CurrentIndex == TotalQuestions - 1 && _currentAnswered;
    public Exercise Current => _exercises[CurrentIndex];

    public bool Submit(bool correct)
    {
        if (_currentAnswered) throw new InvalidOperationException("Ответ уже учтён.");
        _currentAnswered = true;
        if (correct) CorrectAnswers++;
        return correct;
    }

    public bool MoveNext()
    {
        if (!_currentAnswered) return false;
        if (IsFinished) return false;
        CurrentIndex++;
        _currentAnswered = false;
        return true;
    }

    public SessionResult Finish(DateTime? completedAt = null)
    {
        if (!IsFinished) throw new InvalidOperationException("Сессия ещё не завершена.");
        var end = completedAt ?? DateTime.Now;
        return new SessionResult(end, CorrectAnswers, TotalQuestions, end - _startedAt);
    }
}
