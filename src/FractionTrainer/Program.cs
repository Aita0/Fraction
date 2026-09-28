using FractionTrainer.Services;
using FractionTrainer.UI;

namespace FractionTrainer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var storage = new JsonStorage();
        var (settings, warning) = storage.LoadSettings();
        using var form = new MainForm(storage, settings);
        if (warning is not null)
            form.Shown += (_, _) => MessageBox.Show(form, warning, "Чтение данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        Application.Run(form);
    }
}
