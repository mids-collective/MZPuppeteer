using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Plugin;

public class ConfigFile : IPluginConfiguration
{
    public HashSet<string> AuthorizedUsers = new();
    public string TriggerWord = "Trigger";
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public HashSet<XivChatType> AllowedChats = new();
    public HashSet<string> CommandBlocklist = new() { "tell", "say", "shout", "yell" };
    public int Version { get; set; } = 1;
    public bool AllowConfigLocking = false;
    public bool ConfigLocked = true;
}