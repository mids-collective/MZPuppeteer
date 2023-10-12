using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace MZPuppeteer.Services;
public sealed class PuppetService : IDisposable
{
    public static PuppetService Instance => Service<PuppetService>.Instance;
    public ConfigFile config => Service<ConfigService>.Instance.Configuration!;
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
                    if (cmd.StartsWith("user"))
                    {
                        switch (cmd.Split(" ")[1])
                        {
                            case "unlock":
                                config.ConfigAllowed = true;
                                ConfigService.Instance.Save();
                                break;
                            case "lock":
                                config.ConfigAllowed = false;
                                ConfigService.Instance.CloseConfig();
                                DalamudApi.PluginInterface.SavePluginConfig(config);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!config.CommandBlocklist.Contains(cmd.Split(" ")[0]))
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