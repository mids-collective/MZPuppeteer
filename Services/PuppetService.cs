using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace Plugin.Services;
public sealed class PuppetService : IService<PuppetService>
{
    public static PuppetService Instance => Service<PuppetService>.Instance;
    private static ConfigService ConfigSrv => ConfigService.Instance;
    private static ConfigFile Config => ConfigSrv.Configuration;
    private PuppetService()
    {
        DalamudApi.Chat.ChatMessage += Puppeter;
    }
    public void Puppeter(XivChatType chattype, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Config.AllowedChats.Contains(chattype))
        {
            if (Config.AuthorizedUsers2.HasUser(sender.TextValue))
            {
                var user = Config.AuthorizedUsers2.GetUser(sender.TextValue);
                if (message.TextValue.StartsWith($"{user.Triggerword} "))
                {
                    var cmd = message.TextValue.Replace($"{user.Triggerword} ", "");
                    if (!Config.CommandBlocklist.Contains(cmd.Split(" ")[0]) || Config.CommandBlocklist.Contains(cmd))
                    {
                        if (cmd.StartsWith("user"))
                        {
                            switch (cmd.Split(" ")[1])
                            {
                                case "unlock":
                                    if (user.HasPermission(UserPermissions.AllowConfigLocking))
                                        ConfigSrv.SetConfigLock(false);
                                    break;
                                case "lock":
                                    if (user.HasPermission(UserPermissions.AllowConfigLocking))
                                        ConfigSrv.SetConfigLock(true);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if (user.HasPermission(UserPermissions.AllowUseCommands))
                            {
                                CmdService.Execute($"/{cmd}");
                            }
                        }
                    }
                }
            }
        }
    }
    public void Dispose()
    {
        DalamudApi.Chat.ChatMessage -= Puppeter;
    }
}