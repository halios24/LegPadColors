using System.IO;
using System.Text.Json;
using UnityEngine.InputSystem;

namespace LegPadColors;

public class ModSettings
{

    // Color components
    
    // Red team
    public float RedTeamPadRedValue { get; set; } = 0.8f;
    public float RedTeamPadGreenValue { get; set; } = 0.2f;
    public float RedTeamPadBlueValue { get; set; } = 0.2f;
    
    // Blue team
    public float BlueTeamPadRedValue { get; set; } = 0f;
    public float BlueTeamPadGreenValue { get; set; } = 0.549f;
    public float BlueTeamPadBlueValue { get; set; } = 0.804f;


    static string ConfigurationFileName = $"{Plugin.MOD_NAME}.json";

    public static ModSettings Load()
    {
        var path = GetConfigPath();
        var dir = Path.GetDirectoryName(path);

        // 1) make sure "config/" exists
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            Plugin.Log($"Created missing /config directory");
        }

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ModSettings>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                return settings ?? new ModSettings();
            }
            catch (JsonException je)
            {
                Plugin.Log($"Corrupt config JSON, using defaults: {je.Message}");
                return new ModSettings();
            }
        }

        var defaults = new ModSettings();
        File.WriteAllText(path,
            JsonSerializer.Serialize(defaults, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        Plugin.Log($"Config file `{path}` did not exist, created with defaults.");
        return defaults;
    }

    public void Save()
    {
        var path = GetConfigPath();
        var dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path,
            JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
    }

    public static string GetConfigPath()
    {
        string rootPath = Path.GetFullPath(".");
        string configPath = Path.Combine(rootPath, "config", ConfigurationFileName);
        return configPath;
    }
}