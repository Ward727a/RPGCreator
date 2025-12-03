using RPGCreator.Core.Types.Editor;
using Serilog;

namespace RPGCreator.Core;

public class EngineIcons
{
    private readonly string ICONS_PATH = $"{AppContext.BaseDirectory}Assets/Icons/";
    private readonly string ICONS_LICENSE_PATH = $"{AppContext.BaseDirectory}Assets/Icons/icon_license";

    public string IconsLicense = string.Empty;
    
    public HashSet<IconMeta> IconsMeta { get; set; } = new HashSet<IconMeta>();
    
    internal EngineIcons()
    {
        
        #if !DEBUG
        Log.Error("EngineIcons should only be initialized in DEBUG mode.");
        return;
        #endif
        
        if(!Directory.Exists(ICONS_PATH))
        {
            Log.Warning("Icons directory not found at path: {ICONS_PATH}", ICONS_PATH);
            return;
        }

        foreach (var meta_path in Directory.GetFiles(ICONS_PATH, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(meta_path);
                // Convert data to JsonObject
                var iconMeta = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IconMeta>>(json);
                if(iconMeta != null)
                {
                    Log.Information("Loaded {count} icon metadata entries from file: {meta_path}", iconMeta.Count, meta_path);
                    IconsMeta = IconsMeta.Union(iconMeta).ToHashSet();
                }
                else
                {
                    Log.Warning("Failed to deserialize icon metadata from file: {meta_path}", meta_path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load icon metadata from file: {meta_path}", meta_path);
            }
        }
        
        if (File.Exists(ICONS_LICENSE_PATH))
        {
            IconsLicense = File.ReadAllText(ICONS_LICENSE_PATH);
            Log.Information("Loaded icons license from file.");
        }
        else
        {
            Log.Warning("Icons license file not found at path: {ICONS_LICENSED_PATH}", ICONS_LICENSE_PATH);
        }

        Log.Information("EngineIcons initialized.");
    }
}