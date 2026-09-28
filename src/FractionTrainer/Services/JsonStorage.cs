using System.Text.Json;
using FractionTrainer.Models;

namespace FractionTrainer.Services;

public sealed class JsonStorage
{
    private readonly string _folder;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonStorage(string? folder = null) => _folder = folder ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FractionTrainer");

    public (AppSettings Settings, string? Warning) LoadSettings()
    {
        try
        {
            string path = Path.Combine(_folder, "settings.json");
            if (!File.Exists(path)) return (new AppSettings(), null);
            return (JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), _options) ?? new AppSettings(), null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return (new AppSettings(), "Файл настроек повреждён или недоступен. Использованы настройки по умолчанию.");
        }
    }

    public IReadOnlyList<SessionResult> LoadResults(out string? warning)
    {
        warning = null;
        try
        {
            string path = Path.Combine(_folder, "results.json");
            if (!File.Exists(path)) return [];
            return JsonSerializer.Deserialize<List<SessionResult>>(File.ReadAllText(path), _options) ?? [];
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            warning = "Историю результатов прочитать не удалось. Показан пустой список.";
            return [];
        }
    }

    public bool SaveSettings(AppSettings settings, out string? error) => Save("settings.json", settings, out error);

    public bool AddResult(SessionResult result, out string? error)
    {
        var results = LoadResults(out _).ToList();
        results.Add(result);
        return Save("results.json", results, out error);
    }

    private bool Save<T>(string fileName, T value, out string? error)
    {
        try
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Path.Combine(_folder, fileName), JsonSerializer.Serialize(value, _options));
            error = null;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            error = "Данные сохранить не удалось. Приложение продолжит работу.";
            return false;
        }
    }
}
