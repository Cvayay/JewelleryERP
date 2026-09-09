using System.IO;
using System.Windows;

namespace JewelleryERP.Services;

public sealed class ThemeService
{
    private const string LightTheme = "LightTheme.xaml";
    private const string DarkTheme = "DarkTheme.xaml";
    private readonly string _themeStatePath = Path.Combine(Helpers.AppPaths.AppDataRoot, "theme.txt");

    public bool IsDark { get; private set; }

    public string DisplayName => IsDark ? "Dark mode" : "Light mode";

    public string ToggleLabel => IsDark ? "Switch to light mode" : "Switch to dark mode";

    public void ApplySavedTheme()
    {
        var savedTheme = File.Exists(_themeStatePath)
            ? File.ReadAllText(_themeStatePath).Trim()
            : string.Empty;

        ApplyTheme(string.Equals(savedTheme, "dark", StringComparison.OrdinalIgnoreCase));
    }

    public void ToggleTheme()
    {
        ApplyTheme(!IsDark);
    }

    private void ApplyTheme(bool dark)
    {
        if (Application.Current is null) return;

        Helpers.AppPaths.EnsureCreated();
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var themeDictionary = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) == true ||
            dictionary.Source?.OriginalString.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) == true);

        var replacement = new ResourceDictionary
        {
            Source = new Uri($"/JewelleryERP;component/Themes/{(dark ? DarkTheme : LightTheme)}", UriKind.Relative)
        };

        if (themeDictionary is not null)
        {
            var index = dictionaries.IndexOf(themeDictionary);
            dictionaries[index] = replacement;
        }
        else
        {
            dictionaries.Insert(0, replacement);
        }

        IsDark = dark;
        File.WriteAllText(_themeStatePath, dark ? "dark" : "light");
    }
}
