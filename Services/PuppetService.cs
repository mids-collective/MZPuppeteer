using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace Plugin.Services;
public sealed class PuppetService : IService<PuppetService>
{
    public static PuppetService Instance => Service<PuppetService>.Instance;
    private ConfigService srv => Service<ConfigService>.Instance;
    private ConfigFile config => srv.Configuration;

    private PuppetService()
    {
        DalamudApi.Chat.ChatMessage += Puppeter;
    }
    private void Puppeter(XivChatType chattype, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (config.AllowedChats.Contains(chattype))
        {
            if (config.AuthorizedUsers.Contains(sender.TextValue) || chattype == XivChatType.Echo)
            {
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
                                    srv.SetConfigLock(false);
                                    break;
                                case "lock":
                                    if (config.AllowConfigLocking)
                                    {
                                        srv.SetConfigLock(true);
                                        srv.CloseConfig();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            CmdService.Instance.ExecuteCommand($"/{cmd}");
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