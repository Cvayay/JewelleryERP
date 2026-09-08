using System.IO;

namespace JewelleryERP.Helpers;

public static class AppPaths
{
    public static string AppDataRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "JewelleryERP");

    public static string DatabasePath => Path.Combine(AppDataRoot, "jewelleryerp.db");

    public static string ExportsFolder => Path.Combine(AppDataRoot, "Exports");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(ExportsFolder);
    }

    public static void WriteError(string source, Exception exception)
    {
        EnsureCreated();
        var logPath = Path.Combine(AppDataRoot, "error.log");
        var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{source}] {exception}\n";
        File.AppendAllText(logPath, message);
    }
}
