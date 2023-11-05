using System.Reflection;
using Newtonsoft.Json;

namespace Plugin.Services;

public sealed class Localization : IService<Localization>
{
    public static Localization Instance => Service<Localization>.Instance;
    public static string Localize(string glob) => Instance.LocalizeString(glob);
    private Dictionary<string, string> locale = new();
    public string LocalizeString(string Glob)
    {
        if (locale.ContainsKey($"{DalamudApi.PluginInterface.UiLanguage}_{Glob}"))
        {
            return locale[$"{DalamudApi.PluginInterface.UiLanguage}_{Glob}"];
        }
        else
        {
            return Glob;
        }
    }
    private Localization()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        using (var stream = thisAssembly.GetManifestResourceStream($"{thisAssembly.GetName().Name}.locale.json"))
        {
            if (stream != null)
                using (var reader = new StreamReader(stream))
                {
                    if (reader != null)
                        locale = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd()) ?? new();
                }
        }
    }
    public void Dispose()
    {
    }
}