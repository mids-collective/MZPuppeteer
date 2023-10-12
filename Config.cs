using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MZPuppeteer;

public class ConfigFile : IPluginConfiguration
{
    public HashSet<String> AuthorizedUsers = new();
    public String TriggerWord = "Trigger";
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public HashSet<XivChatType> AllowedChats = new();
    public HashSet<String> CommandBlocklist = new() { "tell", "say", "shout", "yell" };
    public int Version { get; set; } = 1;
    public bool ConfigAllowed = true;
}