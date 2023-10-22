using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Plugin;

[Flags]
public enum UserPermissions : ushort
{
    None = 0,
    AllowConfigLocking = 1,
    AllowUseCommands = 2
}

public class User : IEqualityComparer<User>
{
    public string Name = string.Empty;
    public UserPermissions Perms = UserPermissions.None;
    public string Triggerword = string.Empty;
    public User() { }
    public User(string Name)
    {
        this.Name = Name;
    }
    public bool HasPermission(UserPermissions permmission) => Perms.HasFlag(permmission);
    public void TogglePermission(UserPermissions perm) => Perms ^= perm;
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