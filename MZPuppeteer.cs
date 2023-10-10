using System;
using System.Runtime.InteropServices;

using Dalamud;
using Dalamud.Plugin;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;

using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

using ImGuiNET;

using MZPuppeteer.Structures;
using MZPuppeteer.Attributes;

namespace MZPuppeteer;

public unsafe class MZPuppeter : IDalamudPlugin
{
    public static MZPuppeter? Instance;
    public string Name => "MZPuppeteer";
    private Puppeter Configuration;
    private PluginCommandManager commandManager;
    private UIModule* uiModule;
    // Macro Execution
    public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, nint macro);
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28")]
    public static Hook<ExecuteMacroDelegate>? ExecuteMacroHook;
    public static RaptureShellModule* raptureShellModule;
    public static RaptureMacroModule* raptureMacroModule;
    //Extended Macro Execution
    private static nint numCopiedMacroLinesPtr = nint.Zero;
    public static byte NumCopiedMacroLines
    {
        get => *(byte*)numCopiedMacroLinesPtr;
        set
        {
            if (numCopiedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
        }
    }
    private static nint numExecutedMacroLinesPtr = nint.Zero;
    public static byte NumExecutedMacroLines
    {
        get => *(byte*)numExecutedMacroLinesPtr;
        set
        {
            if (numExecutedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
        }
    }
    // Command Execution
    public delegate void ProcessChatBoxDelegate(UIModule* uiModule, nint message, nint unused, byte a4);
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
    public static ProcessChatBoxDelegate? ProcessChatBox;
    private bool config_open = false;
    private string CharacterToAdd = String.Empty;
    private string Command = String.Empty;
    private void DrawConfig()
    {
        if (config_open)
        {
            var changed = false;
            if (ImGui.Begin("MZ Puppeteer Plugin", ref config_open, ImGuiWindowFlags.None))
            {
                ImGui.BeginTabBar("Main");
                if (ImGui.BeginTabItem("Basic"))
                {
                    ImGui.InputText("Trigger Word", ref Configuration.TriggerWord, 0xF);
                    ImGui.SameLine();
                    if (ImGui.Button("Save"))
                    {
                        changed = true;
                    }
                    if (ImGui.Button("Lock Config"))
                    {
                        Configuration.ConfigAllowed = false;
                        config_open = false;
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
                            Configuration.AuthorizedUsers.Add(CharacterToAdd);
                            CharacterToAdd = string.Empty;
                            changed = true;
                        }
                    }
                    ImGui.Text("Currently Allowed Characters");
                    foreach (var chr in Configuration.AuthorizedUsers)
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
                            Configuration.CommandBlocklist.Add(Command);
                            Command = string.Empty;
                            changed = true;
                        }
                    }
                    ImGui.Text("Currently Blocked Commands");
                    foreach (var chr in Configuration.CommandBlocklist)
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
                        var cont = Configuration.AllowedChats.Contains(chan);
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
                ImGui.End();
            }
            if (changed)
            {
                DalamudApi.PluginInterface.SavePluginConfig(Configuration);
            }
        }
    }
    public void Puppeter(XivChatType chattype, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Configuration.AllowedChats.Contains(chattype))
        {
            if (Configuration.AuthorizedUsers.Contains(sender.TextValue) || chattype == XivChatType.Echo)
            {
                if (message.TextValue.StartsWith($"{Configuration.TriggerWord} "))
                {
                    var cmd = message.TextValue.Replace($"{Configuration.TriggerWord} ", "");
                    if (cmd.StartsWith("user"))
                    {
                        switch (cmd.Split(" ")[1])
                        {
                            case "unlock":
                                Configuration.ConfigAllowed = true;
                                DalamudApi.PluginInterface.SavePluginConfig(Configuration);
                                break;
                            case "lock":
                                Configuration.ConfigAllowed = false;
                                config_open = false;
                                DalamudApi.PluginInterface.SavePluginConfig(Configuration);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!Configuration.CommandBlocklist.Contains(cmd.Split(" ")[0]))
                        {
                            ExecuteCommand($"/{cmd}");
                        }
                    }
                }
            }
        }
    }
    public MZPuppeter(DalamudPluginInterface dpi)
    {
        Instance = this;
        DalamudApi.Initialize(dpi);
        commandManager = new PluginCommandManager(this);
        Configuration = (Puppeter?)DalamudApi.PluginInterface.GetPluginConfig() ?? new();
        DalamudApi.PluginInterface.SavePluginConfig(Configuration);
        DalamudApi.PluginInterface.UiBuilder.Draw += DrawConfig;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        if (DalamudApi.ClientState.IsLoggedIn)
        {
            InitCommands();
        }
        DalamudApi.ClientState.Login += InitCommands;
        DalamudApi.Chat.ChatMessage += Puppeter;
    }
    private void ToggleConfig()
    {
        if (Configuration.ConfigAllowed)
        {
            config_open = !config_open;
        }
        else
        {
            config_open = false;
            DalamudApi.Chat.Print(new XivChatEntry
            {
                Message = "Configuration is locked, get your Dom to unlock.",
                Type = DalamudApi.PluginInterface.GeneralChatType,
                SenderId = 0,
                Name = String.Empty,
            });
        }
    }
    [Command("/puppeteer")]
    [HelpMessage("Show or hide plugin configuation")]
    private void ToggleConfig(string cmd, string args)
    {
        ToggleConfig();
    }
    private void InitCommands()
    {
        uiModule = Framework.Instance()->GetUiModule();
        raptureShellModule = uiModule->GetRaptureShellModule();
        raptureMacroModule = uiModule->GetRaptureMacroModule();
        DalamudApi.GameInteropProvider.InitializeFromAttributes(this);
        numCopiedMacroLinesPtr = DalamudApi.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
        numExecutedMacroLinesPtr = DalamudApi.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;
    }

    public void ExecuteCommand(string command)
    {
        if (ProcessChatBox == null)
        {
            InitCommands();
        }
        if (command.StartsWith("//"))
        {
            command = command[2..].ToLower();
            switch (command[0])
            {
                case 'm':
                    int.TryParse(command[1..], out int val);
                    if (val is >= 0 and < 200)
                    {
                        ExecuteMacroHook!.Original(raptureShellModule, (nint)raptureMacroModule + 0x58 + (Macro.size * val));
                    }
                    break;
            }
            return;
        }
        var stringPtr = nint.Zero;
        try
        {
            stringPtr = Marshal.AllocHGlobal(UTF8String.size);
            using var str = new UTF8String(stringPtr, command);
            Marshal.StructureToPtr(str, stringPtr, false);
            ProcessChatBox!(uiModule, stringPtr, nint.Zero, 0);
        }
        catch
        {
            DalamudApi.PluginLog.Error("Error with injecting command");
        }
        Marshal.FreeHGlobal(stringPtr);
    }

    public void Dispose()
    {
        NumCopiedMacroLines = 15;
        NumExecutedMacroLines = 15;
        DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        DalamudApi.PluginInterface.UiBuilder.Draw -= DrawConfig;
        DalamudApi.ClientState.Login -= InitCommands;
        DalamudApi.Chat.ChatMessage -= Puppeter;
        commandManager?.Dispose();
        ExecuteMacroHook?.Dispose();
    }

    public static void ExecuteMacroDetour(RaptureShellModule* raptureShellModule, nint macro)
    {
        NumCopiedMacroLines = Macro.numLines;
        NumExecutedMacroLines = Macro.numLines;
        ExecuteMacroHook!.Original(raptureShellModule, macro);
    }
}