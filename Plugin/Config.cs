using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Interface.Internal.Windows.Settings.Widgets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Plugin;

[Flags]
public enum UserPermissions
{
    None = 0,
    AllowConfigLocking = 1
}

public class User : IEqualityComparer<User>
{
    public string Name;
    private UserPermissions Perms;
    public User()
    {
        Name = string.Empty;
        Perms = UserPermissions.None;
    }
    public User(string Name)
    {
        this.Name = Name;
        this.Perms = UserPermissions.None;
    }
    public bool HasPermission(UserPermissions permmission) => Perms.HasFlag(permmission);
    public void TogglePermission(UserPermissions perm) => Perms &= perm;
    public bool Equals(User? x, User? y)
    {
        return x != null && y != null && (x.Name == y.Name);
    }
    public int GetHashCode(User obj)
    {
        return obj.Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}

public class ConfigFile : IPluginConfiguration
{
    public HashSet<string> AuthorizedUsers = new();
    public HashSet<User> AuthorizedUsers2 = new();
    public string TriggerWord = "Trigger";
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public HashSet<XivChatType> AllowedChats = new();
    public HashSet<string> CommandBlocklist = new() { "tell", "say", "shout", "yell" };
    public bool AllowConfigLocking = false;
    public bool ConfigLocked = false;
    public int Version { get; set; } = 2;
}

public static class ConfigExtensions
{
    public static bool HasUser(this HashSet<User> hs, string user) => hs.Where(x => x.Name == user).Count() != 0;
    public static User GetUser(this HashSet<User> hs, string user) => hs.Where(x => x.Name == user).First();
    public static bool UserHasPermission(this HashSet<User> hs, UserPermissions perm, string user) => hs.Where(x => x.Name == user).First().HasPermission(perm);
}