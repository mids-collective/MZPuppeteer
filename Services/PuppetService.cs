using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace Plugin.Services;
public sealed class PuppetService : IService<PuppetService>
{
    public static PuppetService Instance => Service<PuppetService>.Instance;
    private ConfigService ConfigSrv => Service<ConfigService>.Instance;
    private ConfigFile config => ConfigSrv.Configuration;

    private PuppetService()
    {
        DalamudApi.Chat.ChatMessage += Puppeter;
    }
    private void Puppeter(XivChatType chattype, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (config.AllowedChats.Contains(chattype))
        {
            if (config.AuthorizedUsers2.HasUser(sender.TextValue) || chattype == XivChatType.Echo)
            {
                var user = config.AuthorizedUsers2.GetUser(sender.TextValue);
                if (message.TextValue.StartsWith($"{config.TriggerWord} "))
                {
                    var cmd = message.TextValue.Replace($"{config.TriggerWord} ", "");
                    if (!config.CommandBlocklist.Contains(cmd.Split(" ")[0]) || config.CommandBlocklist.Contains(cmd))
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
                            CmdService.Execute($"/{cmd}");
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