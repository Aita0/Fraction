using FractionTrainer.Logic;
using FractionTrainer.Models;
using FractionTrainer.Services;

namespace FractionTrainer.UI;

public sealed class MainForm : Form
{
    private readonly JsonStorage _storage;
    private readonly AppSettings _settings;

    public MainForm(JsonStorage storage, AppSettings settings)
    {
        _storage = storage;
        _settings = settings;
        Text = "Тренажёр «Собрать дробь»";
        MinimumSize = new Size(800, 650);
        Size = new Size(1000, 760);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 11);
        ShowMenu();
    }

    private void ShowMenu()
    {
        Controls.Clear();
        var title = new Label { Text = "Тренажёр «Собрать дробь»", Font = new Font(Font.FontFamily, 26, FontStyle.Bold), AutoSize = true, Anchor = AnchorStyles.None };
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Anchor = AnchorStyles.None };
        AddMenuButton(buttons, "Обучение", () => ShowExercise(false));
        AddMenuButton(buttons, "Проверка знаний", () => ShowExercise(true));
        AddMenuButton(buttons, "Настройки", ShowSettings);
        AddMenuButton(buttons, "Результаты", ShowResults);
        AddMenuButton(buttons, "Выход", Close);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        layout.Controls.Add(title, 0, 0); layout.Controls.Add(buttons, 0, 1);
        Controls.Add(layout);
    }

    private static void AddMenuButton(Control parent, string text, Action click)
    {
        var button = new Button { Text = text, Width = 300, Height = 55, Margin = new Padding(8) };
        button.Click += (_, _) => click(); parent.Controls.Add(button);
    }

    private void ShowExercise(bool knowledgeMode)
    {
        Controls.Clear();
        var screen = new ExerciseScreen(_settings, knowledgeMode, _storage);
        screen.GoToMenu += (_, _) => ShowMenu();
        Controls.Add(screen);
    }

    private void ShowSettings()
    {
        Controls.Clear();
        var kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 270 };
        kind.Items.AddRange(new object[] { "Случайная фигура", "Круг", "Прямоугольные полосы", "Прямоугольная сетка" });
        kind.SelectedIndex = (int)_settings.ShapeKind;
        var colorPanel = new Panel { BackColor = _settings.SelectedColor, Width = 270, Height = 45, BorderStyle = BorderStyle.FixedSingle };
        var choose = new Button { Text = "Выбрать цвет…", AutoSize = true };
        choose.Click += (_, _) => { using var dialog = new ColorDialog { Color = colorPanel.BackColor }; if (dialog.ShowDialog(this) == DialogResult.OK) colorPanel.BackColor = dialog.Color; };
        var save = new Button { Text = "Сохранить", Width = 180, Height = 45 };
        save.Click += (_, _) =>
        {
            _settings.ShapeKind = (ShapeKind)kind.SelectedIndex; _settings.SelectedColorArgb = colorPanel.BackColor.ToArgb();
            if (!_storage.SaveSettings(_settings, out var error)) MessageBox.Show(this, error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else ShowMenu();
        };
        var back = new Button { Text = "Назад", Width = 180, Height = 45 }; back.Click += (_, _) => ShowMenu();
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(80), WrapContents = false };
        panel.Controls.Add(new Label { Text = "Настройки", Font = new Font(Font.FontFamily, 24, FontStyle.Bold), AutoSize = true });
        panel.Controls.Add(new Label { Text = "Вид фигуры:", AutoSize = true, Margin = new Padding(3, 30, 3, 3) }); panel.Controls.Add(kind);
        panel.Controls.Add(new Label { Text = "Цвет выбранных частей:", AutoSize = true, Margin = new Padding(3, 20, 3, 3) }); panel.Controls.Add(colorPanel); panel.Controls.Add(choose); panel.Controls.Add(save); panel.Controls.Add(back);
        Controls.Add(panel);
    }

    private void ShowResults()
    {
        Controls.Clear();
        var results = _storage.LoadResults(out var warning).OrderByDescending(x => x.CompletedAt).ToList();
        var table = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        table.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Дата и время", DataPropertyName = "Date" });
        table.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Правильно", DataPropertyName = "Score" });
        table.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Процент", DataPropertyName = "Percent" });
        table.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Длительность", DataPropertyName = "Duration" });
        table.DataSource = results.Select(x => new { Date = x.CompletedAt.ToString("g"), Score = $"{x.CorrectAnswers} из {x.TotalQuestions}", Percent = $"{x.Percent}%", Duration = x.Duration.ToString(@"mm\:ss") }).ToList();
        var back = new Button { Text = "В меню", Dock = DockStyle.Bottom, Height = 50 }; back.Click += (_, _) => ShowMenu();
        Controls.Add(table); Controls.Add(back);
        if (warning is not null) BeginInvoke(() => MessageBox.Show(this, warning, "Чтение данных", MessageBoxButtons.OK, MessageBoxIcon.Warning));
    }
}
