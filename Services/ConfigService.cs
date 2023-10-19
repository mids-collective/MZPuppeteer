using ImGuiNET;

using Dalamud.Utility;
using Dalamud.Game.Text;
namespace Plugin.Services;

public sealed class ConfigService : IService<ConfigService>
{
    public static ConfigService Instance => Service<ConfigService>.Instance;
    public ConfigFile Configuration;
    private bool ConfigOpen = false;
    private void ConvertTo2()
    {
        foreach (var user in Configuration.AuthorizedUsers)
        {
            Configuration.AuthorizedUsers2.Add(new User(user));
        }
        Configuration.Version = 2;
    }
    private ConfigService()
    {
        Configuration = (ConfigFile?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
        if (Configuration.Version < 2)
        {
            ConvertTo2();
        }
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
    public void SetConfigLock(bool configLocked)
    {
        if (Configuration.AllowConfigLocking)
        {
            Configuration.ConfigLocked = configLocked;
            CloseConfig();
        }
        else
        {
            Configuration.ConfigLocked = false;
        }
        DalamudApi.PluginInterface.SavePluginConfig(Configuration);
    }

    private bool DrawBasicTab()
    {
        var changed = false;

        if (ImGui.BeginTabItem("Basic Configuration"))
        {
            ImGui.InputText("Trigger Word", ref Configuration!.TriggerWord, 0x14);
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                changed = true;
            }
            ImGui.EndTabItem();
        }
        return changed;
    }

    public bool DrawCharacterTab()
    {
        var changed = false;
        if (ImGui.BeginTabItem("Allowed Characters"))
        {
            ImGui.InputText("Character: ", ref CharacterToAdd, 20);
            ImGui.SameLine();
            if (ImGui.Button("+"))
            {
                if (!CharacterToAdd.IsNullOrWhitespace())
                {
                    if (Configuration!.AuthorizedUsers2.Add(new User(CharacterToAdd)))
                    {
                        CharacterToAdd = string.Empty;
                        changed = true;
                    }
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Add Targeted Character"))
            {
                if (DalamudApi.TargetManager.Target != null)
                {
                    if (Configuration!.AuthorizedUsers2.Add(new User(DalamudApi.TargetManager.Target.Name.ToString())))
                    {
                        changed = true;
                    }
                }
            }
            ImGui.Text("Currently Allowed Characters");
            foreach (var chr in Configuration!.AuthorizedUsers2)
            {
                if (ImGui.Button($"X##{chr}"))
                {
                    Configuration.AuthorizedUsers2.Remove(chr);
                    changed = true;
                }
                ImGui.SameLine();
                ImGui.Text($"{chr}");
                ImGui.Text("Permissions");
                foreach (var en in Enum.GetNames<UserPermissions>())
                {
                    if (en.Equals("None"))
                    {
                        continue;
                    }
                    ImGui.SameLine();
                    var Perm = Enum.Parse<UserPermissions>(en);
                    var HasPerm = chr.HasPermission(Perm);
                    var HPC = HasPerm;
                    ImGui.Checkbox($"##{en}", ref HasPerm);
                    if (HPC != HasPerm)
                    {
                        chr.TogglePermission(Perm);
                        changed = true;
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"{Localization.Localize(en)}");
                    }
                }
                ImGui.Separator();
            }
            ImGui.EndTabItem();
        }
        return changed;
    }

    private bool DrawBlocklistTab()
    {
        var changed = false;
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
            var old = Configuration.AllowConfigLocking;
            ImGui.Checkbox("Allow Config Locking", ref Configuration.AllowConfigLocking);
            if (old != Configuration.AllowConfigLocking)
            {
                changed = true;
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("WARNING: Enabling this can cause you to lose control completely!!");
            }
            foreach (var cmd in Configuration!.CommandBlocklist)
            {
                if (ImGui.Button($"X##{cmd}"))
                {
                    Configuration.CommandBlocklist.Remove(cmd);
                    changed = true;
                }
                ImGui.SameLine();
                ImGui.Text($"{cmd}");
            }
            ImGui.EndTabItem();
        }
        return changed;
    }

    public bool DrawChatChannels()
    {
        bool changed = false;
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
        return changed;
    }
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
                changed |= DrawBasicTab();
                changed |= DrawCharacterTab();
                changed |= DrawBlocklistTab();
                changed |= DrawChatChannels();
                ImGui.EndTabBar();
                ImGui.EndChild();
                ImGui.Separator();
                if (Configuration.AllowConfigLocking)
                {
                    if (ImGui.Button("Lock Config"))
                    {
                        SetConfigLock(true);
                    }
                }
                ImGui.End();
            }
            if (Configuration.ConfigLocked && !Configuration.AllowConfigLocking && !Configuration.AuthorizedUsers2.Any(x => x.HasPermission(UserPermissions.AllowConfigLocking)))
            {
                Configuration.ConfigLocked = false;
                changed = true;
            }
            if (changed)
            {
                DalamudApi.PluginInterface.SavePluginConfig(Configuration);
            }
        }
    }

    public void ToggleConfig()
    {
        if (!(Configuration!.ConfigLocked && Configuration.AllowConfigLocking) || !Configuration.AuthorizedUsers2.Any(x => x.HasPermission(UserPermissions.AllowConfigLocking)))
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
                Name = string.Empty,
            });
        }
    }

    public void CloseConfig()
    {
        ConfigOpen = false;
    }
    public void OpenConfig()
    {
        if (!Configuration!.ConfigLocked)
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