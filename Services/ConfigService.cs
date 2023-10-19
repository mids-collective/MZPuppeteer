using ImGuiNET;

using Dalamud.Utility;
using Dalamud.Game.Text;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Plugin.Services;

public sealed class ConfigService : IService<ConfigService>
{
    public static ConfigService Instance => Service<ConfigService>.Instance;
    public ConfigFile Configuration;
    private bool ConfigOpen = false;
    private ConfigService()
    {
        Configuration = (ConfigFile?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
        DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        CmdMgrService.Instance.AddCommand("/puppeteer", ToggleConfig, "Toggle Configuration Window");
    }
    private void ToggleConfig(string cmd, string args)
    {
        ToggleConfig();
    }
    private string CharacterToAdd = string.Empty;
    private string Command = string.Empty;
    private void Draw()
    {
        if (ConfigOpen)
        {
            var changed = false;
            if (ImGui.Begin("MZ Puppeteer Plugin", ref ConfigOpen, ImGuiWindowFlags.None))
            {
                var size = ImGui.GetContentRegionAvail();
                size.Y -= 30;
                ImGui.BeginChild("Puppeteer", size);
                ImGui.BeginTabBar("Main");
                if (ImGui.BeginTabItem("Basic"))
                {
                    ImGui.InputText("Trigger Word", ref Configuration!.TriggerWord, 0x14);
                    ImGui.SameLine();
                    if (ImGui.Button("Save"))
                    {
                        changed = true;
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Allowed Characters"))
                {
                    ImGui.InputText("Character: ", ref CharacterToAdd, 20);
                    ImGui.SameLine();
                    if (ImGui.Button("+"))
                    {
                        if (!CharacterToAdd.IsNullOrWhitespace())
                        {
                            Configuration!.AuthorizedUsers.Add(CharacterToAdd);
                            CharacterToAdd = string.Empty;
                            changed = true;
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Add Targeted Character"))
                    {
                        if (DalamudApi.TargetManager.Target != null)
                        {
                            Configuration!.AuthorizedUsers.Add(DalamudApi.TargetManager.Target.Name.ToString());
                        }
                    }
                    ImGui.Text("Currently Allowed Characters");
                    foreach (var chr in Configuration!.AuthorizedUsers)
                    {
                        if (ImGui.Button("X"))
                        {
                            Configuration.AuthorizedUsers.Remove(chr);
                            changed = true;
                        }
                        ImGui.SameLine();
                        ImGui.Text($"{chr}");
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Command Blocklist"))
                {
                    ImGui.InputText("Command to Block: ", ref Command, 20);
                    ImGui.SameLine();
                    if (ImGui.Button("+"))
                    {
                        if (!Command.IsNullOrWhitespace())
                        {
                            Configuration!.CommandBlocklist.Add(Command);
                            Command = string.Empty;
                            changed = true;
                        }
                    }
                    ImGui.Text("Currently Blocked Commands");
                    foreach (var chr in Configuration!.CommandBlocklist)
                    {
                        if (ImGui.Button("X"))
                        {
                            Configuration.CommandBlocklist.Remove(chr);
                            changed = true;
                        }
                        ImGui.SameLine();
                        ImGui.Text($"{chr}");
                    }
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Chat Channels"))
                {
                    ImGui.Text("Allowed Chat Channels");
                    foreach (var chan in Enum.GetValues<XivChatType>())
                    {
                        var cont = Configuration!.AllowedChats.Contains(chan);
                        var old = cont;
                        ImGui.Checkbox($"{chan}", ref cont);
                        if (old != cont)
                        {
                            if (!Configuration.AllowedChats.Contains(chan))
                            {
                                Configuration.AllowedChats.Add(chan);
                                changed = true;
                            }
                            else
                            {
                                Configuration.AllowedChats.Remove(chan);
                                changed = true;
                            }
                        }
                    }
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
                ImGui.EndChild();
                ImGui.Separator();
                if (ImGui.Button("Lock Config"))
                {
                    Configuration!.ConfigAllowed = false;
                    ConfigOpen = false;
                    changed = true;
                }
                ImGui.End();
            }
            if (changed)
            {
                DalamudApi.PluginInterface.SavePluginConfig(Configuration);
            }
        }
    }

    public void ToggleConfig()
    {
        if (Configuration!.ConfigAllowed)
        {
            ConfigOpen = !ConfigOpen;
        }
        else
        {
            ConfigOpen = false;
            DalamudApi.Chat.Print(new XivChatEntry
            {
                Message = "Configuration is locked, get your Dom to unlock.",
                Type = DalamudApi.PluginInterface.GeneralChatType,
                SenderId = 0,
                Name = String.Empty,
            });
        }
    }

    public void CloseConfig()
    {
        ConfigOpen = false;
    }
    public void OpenConfig()
    {
        if (Configuration!.ConfigAllowed)
        {
            ConfigOpen = true;
        }
    }
    public void Save()
    {
        DalamudApi.PluginInterface.SavePluginConfig(Configuration);
    }
    public void Dispose()
    {
        DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
    }
}