using FractionTrainer.Drawing;
using FractionTrainer.Logic;
using FractionTrainer.Models;
using FractionTrainer.Services;

namespace FractionTrainer.UI;

public sealed class ExerciseScreen : UserControl
{
    private static readonly Color[] PairColors = [Color.DodgerBlue, Color.DarkOrange, Color.MediumSeaGreen];
    private readonly AppSettings _settings;
    private readonly bool _knowledgeMode;
    private readonly JsonStorage _storage;
    private readonly ExerciseGenerator _generator = new();
    private readonly ShapeCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly FractionView _fraction = new() { Dock = DockStyle.Fill };
    private readonly Label _heading = new() { AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
    private readonly Label _selected = new() { AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
    private readonly Label _feedback = new() { AutoSize = true, MaximumSize = new Size(900, 0) };
    private readonly FlowLayoutPanel _buttons = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private readonly Panel _content = new() { Dock = DockStyle.Fill };
    private readonly List<FractionCardControl> _cardControls = [];
    private readonly List<(int First, int Second)> _pairs = [];
    private readonly HashSet<int> _choices = [];
    private Exercise? _exercise;
    private KnowledgeSession? _session;
    private int? _pendingCard;
    private Button? _submitButton;
    private Button? _nextButton;
    public event EventHandler? GoToMenu;

    public ExerciseScreen(AppSettings settings, bool knowledgeMode, JsonStorage storage)
    {
        _settings = settings; _knowledgeMode = knowledgeMode; _storage = storage; Dock = DockStyle.Fill;
        _canvas.SelectedColor = settings.SelectedColor;
        _canvas.SelectionChanged += (_, _) => UpdateSelectedLabel();
        BuildLayout();
        if (knowledgeMode) StartSession(); else ShowTrainingChooser();
    }

    private void BuildLayout()
    {
        var menu = new Button { Text = "В меню", Dock = DockStyle.Right, Width = 120 }; menu.Click += (_, _) => GoToMenu?.Invoke(this, EventArgs.Empty);
        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 }; top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80)); top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        top.Controls.Add(_heading, 0, 0); top.Controls.Add(menu, 1, 0);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, Padding = new Padding(20) };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
        layout.Controls.Add(top, 0, 0); layout.Controls.Add(_content, 0, 1);
        var info = new FlowLayoutPanel { Dock = DockStyle.Fill }; info.Controls.Add(_selected); info.Controls.Add(_feedback); layout.Controls.Add(info, 0, 2);
        layout.Controls.Add(_buttons, 0, 3); Controls.Add(layout);
    }

    private void ShowTrainingChooser()
    {
        _heading.Text = "Обучение — выберите тип задания"; ResetScreen();
        var chooser = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(80) };
        foreach (var item in new[] { ("Соберите дробь", ExerciseType.Assemble), ("Найдите пары", ExerciseType.FindPairs), ("Выберите все подходящие варианты", ExerciseType.SelectAll) })
        { var button = new Button { Text = item.Item1, Width = 380, Height = 58, Margin = new Padding(8) }; button.Click += (_, _) => StartTraining(item.Item2); chooser.Controls.Add(button); }
        _content.Controls.Add(chooser);
    }

    private void StartTraining(ExerciseType type)
    {
        _exercise = type switch { ExerciseType.Assemble => _generator.CreateAssemble(_settings.ShapeKind), ExerciseType.FindPairs => _generator.CreatePairs(_settings.ShapeKind), _ => _generator.CreateChoices(_settings.ShapeKind) };
        ShowExercise(_exercise);
    }

    private void StartSession() { _session = new KnowledgeSession(_generator.CreateSession(_settings.ShapeKind)); ShowExercise(_session.Current); }

    private void ShowExercise(Exercise exercise)
    {
        _exercise = exercise; ResetScreen();
        _heading.Text = _knowledgeMode ? $"Проверка знаний — задание {_session!.CurrentIndex + 1} из {_session.TotalQuestions}: {TypeName(exercise.Type)}" : $"Обучение: {TypeName(exercise.Type)}";
        if (exercise is AssembleExercise assemble) ShowAssemble(assemble);
        else if (exercise is PairExercise pairs) ShowCards(pairs.Cards, true);
        else if (exercise is ChoiceExercise choices) { ShowTargetAndCards(choices.Target, choices.Cards); }
    }

    private void ResetScreen()
    {
        _content.Controls.Clear(); _buttons.Controls.Clear(); _cardControls.Clear(); _pairs.Clear(); _choices.Clear(); _pendingCard = null;
        _submitButton = null; _nextButton = null;
        _feedback.Text = ""; _selected.Text = ""; _selected.Visible = !_knowledgeMode; _canvas.Visible = true; _fraction.Visible = true;
    }

    private void ShowAssemble(AssembleExercise exercise)
    {
        _fraction.Numerator = exercise.Target.Numerator; _fraction.Denominator = exercise.Target.Denominator; _fraction.Invalidate();
        _canvas.SetRenderer(CreateRenderer(exercise.ShapeKind, exercise.PartCount, exercise.GridRows, exercise.GridColumns)); _canvas.InteractionEnabled = true;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 135)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(_fraction, 0, 0); layout.Controls.Add(_canvas, 0, 1); _content.Controls.Add(layout);
        AddButton("− доля", (_, _) => _canvas.SetSelectedCount(_canvas.SelectedCount - 1)); AddButton("+ доля", (_, _) => _canvas.SetSelectedCount(_canvas.SelectedCount + 1));
        _submitButton = AddButton(_knowledgeMode ? "Ответить" : "Проверить", (_, _) => Submit());
        if (!_knowledgeMode) { AddButton("Очистить", (_, _) => _canvas.ClearSelection()); AddButton("Подсказка", (_, _) => _feedback.Text = $"Фигура — это {_canvas.SelectedCount}/{exercise.PartCount}. Сравни дроби перекрёстным умножением."); }
        UpdateSelectedLabel();
    }

    private void ShowTargetAndCards(Fraction target, IReadOnlyList<FractionCard> cards)
    {
        _fraction.Numerator = target.Numerator; _fraction.Denominator = target.Denominator; _fraction.Invalidate();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 }; layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 125)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(_fraction, 0, 0); var flow = CreateCards(cards, false); layout.Controls.Add(flow, 0, 1); _content.Controls.Add(layout);
        _submitButton = AddButton(_knowledgeMode ? "Ответить" : "Проверить", (_, _) => Submit());
    }

    private void ShowCards(IReadOnlyList<FractionCard> cards, bool pairing)
    {
        _fraction.Visible = false; _content.Controls.Add(CreateCards(cards, pairing));
        _submitButton = AddButton(_knowledgeMode ? "Ответить" : "Проверить", (_, _) => Submit()); _submitButton.Enabled = false;
        if (!_knowledgeMode) _feedback.Text = "Нажми две карточки, чтобы создать пару. Нажми карточку готовой пары, чтобы отменить её.";
    }

    private FlowLayoutPanel CreateCards(IReadOnlyList<FractionCard> cards, bool pairing)
    {
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = true };
        for (int i = 0; i < cards.Count; i++)
        {
            var control = new FractionCardControl(cards[i], i, _settings.SelectedColor); _cardControls.Add(control);
            control.Click += (_, _) => { if (pairing) PairCard(control.CardIndex); else ChooseCard(control.CardIndex); }; flow.Controls.Add(control);
        }
        return flow;
    }

    private void PairCard(int index)
    {
        if (_submitButton?.Enabled == false && _knowledgeMode && _session?.IsCurrentAnswered == true) return;
        int existing = _pairs.FindIndex(p => p.First == index || p.Second == index);
        if (existing >= 0) { _pairs.RemoveAt(existing); _pendingCard = null; RefreshPairs(); return; }
        if (_pendingCard is null) { _pendingCard = index; _cardControls[index].SetChosen(true); return; }
        if (_pendingCard == index) { _pendingCard = null; _cardControls[index].SetChosen(false); return; }
        _pairs.Add((_pendingCard.Value, index)); _pendingCard = null; RefreshPairs();
    }

    private void RefreshPairs()
    {
        foreach (var card in _cardControls) card.SetPair(null, Color.Black);
        for (int i = 0; i < _pairs.Count; i++) { _cardControls[_pairs[i].First].SetPair(i + 1, PairColors[i]); _cardControls[_pairs[i].Second].SetPair(i + 1, PairColors[i]); }
        if (_submitButton is not null) _submitButton.Enabled = _pairs.Count == 3;
    }

    private void ChooseCard(int index)
    {
        if (_choices.Remove(index)) _cardControls[index].SetChosen(false); else { _choices.Add(index); _cardControls[index].SetChosen(true); }
    }

    private void Submit()
    {
        bool correct = _exercise switch
        {
            AssembleExercise a => FractionMath.IsAssembledCorrectly(a, _canvas.SelectedCount),
            PairExercise p => FractionMath.ArePairsCorrect(p, _pairs),
            ChoiceExercise c => FractionMath.AreChoicesCorrect(c, _choices),
            _ => false
        };
        if (_knowledgeMode) { _session!.Submit(correct); LockAnswer(); }
        ShowExplanation(correct);
        if (_nextButton is null)
            _nextButton = _knowledgeMode
                ? AddButton(_session!.IsFinished ? "Показать итог" : "Следующее задание", (_, _) => ContinueSession())
                : AddButton("Следующее задание", (_, _) => ShowTrainingChooser());
    }

    private void LockAnswer()
    {
        _canvas.InteractionEnabled = false; foreach (Button button in _buttons.Controls) button.Enabled = false;
        foreach (var card in _cardControls) card.Enabled = false;
    }

    private void ShowExplanation(bool correct)
    {
        _feedback.ForeColor = correct ? Color.ForestGreen : Color.Firebrick;
        if (_exercise is AssembleExercise a)
            _feedback.Text = $"Ты закрасил {_canvas.SelectedCount} из {a.PartCount} частей. {_canvas.SelectedCount}/{a.PartCount} {(correct ? "=" : "≠")} {a.Target} — {(correct ? "верно!" : "пока неверно. Измени выбор и попробуй снова.")}";
        else if (_exercise is PairExercise) _feedback.Text = correct ? "Верно! Все три пары изображают равные дроби." : "Пока неверно: не все составленные пары равны.";
        else _feedback.Text = correct ? "Верно! Выбраны все равные дроби и ни одной лишней." : "Пока неверно: выбран неполный набор или есть лишняя карточка.";
    }

    private void ContinueSession()
    {
        if (_session!.IsFinished) { ShowSummary(); return; } _session.MoveNext(); ShowExercise(_session.Current);
    }

    private void ShowSummary()
    {
        var result = _session!.Finish(); if (!_storage.AddResult(result, out var error)) MessageBox.Show(this, error, "Сохранение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        ResetScreen(); _heading.Text = "Проверка завершена"; _feedback.ForeColor = Color.Black; _feedback.Text = $"Правильных ответов: {result.CorrectAnswers} из {result.TotalQuestions}\nПроцент: {result.Percent}%\nДлительность: {result.Duration:mm\\:ss}";
        AddButton("Новая сессия", (_, _) => StartSession()); AddButton("В меню", (_, _) => GoToMenu?.Invoke(this, EventArgs.Empty));
    }

    private void UpdateSelectedLabel()
    {
        if (_exercise is AssembleExercise a && !_knowledgeMode) _selected.Text = $"Выбрано: {_canvas.SelectedCount}/{a.PartCount}    ";
    }

    private Button AddButton(string text, EventHandler handler) { var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(145, 44), Margin = new Padding(5) }; button.Click += handler; _buttons.Controls.Add(button); return button; }
    private static string TypeName(ExerciseType type) => type switch { ExerciseType.Assemble => "Соберите дробь", ExerciseType.FindPairs => "Найдите пары", _ => "Выберите все подходящие варианты" };
    private static IShapeRenderer CreateRenderer(ShapeKind kind, int parts, int rows, int columns) => kind switch { ShapeKind.Circle => new CircleRenderer(parts), ShapeKind.Strips => new StripsRenderer(parts), ShapeKind.Grid => new GridRenderer(rows, columns), _ => throw new InvalidOperationException() };
}
