using System.Text.Json;

namespace MES_PRINT;

/// <summary>
/// 配置保存到 %LocalAppData%\MES_PRINT\config.json，打包后仍可写。
/// </summary>
public static class AppConfig
{
    private static string ConfigPath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MES_PRINT");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, "config.json");
        }
    }

    public static string? RouteLaunchFolder { get; set; }
    public static bool PrintLogEnabled { get; set; } = true;
    public static bool BarcodesNotPackedEnabled { get; set; } = true;
    public static bool RouteLaunchEnabled { get; set; } = true;

    public static void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return;
            var json = File.ReadAllText(ConfigPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("RouteLaunchFolder", out var v1) && v1.ValueKind == JsonValueKind.String)
                RouteLaunchFolder = v1.GetString();
            if (root.TryGetProperty("PrintLogEnabled", out var v2))
                PrintLogEnabled = v2.GetBoolean();
            if (root.TryGetProperty("BarcodesNotPackedEnabled", out var v3))
                BarcodesNotPackedEnabled = v3.GetBoolean();
            if (root.TryGetProperty("RouteLaunchEnabled", out var v4))
                RouteLaunchEnabled = v4.GetBoolean();
        }
        catch { /* ignore */ }
    }

    public static void Save()
    {
        try
        {
            var obj = new Dictionary<string, object?>
            {
                ["RouteLaunchFolder"] = RouteLaunchFolder ?? "",
                ["PrintLogEnabled"] = PrintLogEnabled,
                ["BarcodesNotPackedEnabled"] = BarcodesNotPackedEnabled,
                ["RouteLaunchEnabled"] = RouteLaunchEnabled
            };
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { /* ignore */ }
    }
}
