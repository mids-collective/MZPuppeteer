using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Dalamud.Hooking;
using Dalamud;
using Plugin.Structures;

namespace Plugin.Services;

public unsafe sealed class MacroService : IService<MacroService>
{
    public static MacroService Instance => Service<MacroService>.Instance;
    public static void Execute(int id) => Instance.ExecuteMacro(id);
    private const string macroSig = "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 4D 28";
    // Macro Execution
    public delegate void ExecuteMacroDelegate(RaptureShellModule* raptureShellModule, nint macro);
    public Hook<ExecuteMacroDelegate>? ExecuteMacroHook;
    private nint numCopiedMacroLinesPtr = nint.Zero;
    public byte NumCopiedMacroLines
    {
        get => *(byte*)numCopiedMacroLinesPtr;
        set
        {
            if (numCopiedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numCopiedMacroLinesPtr, new[] { value });
        }
    }

    private nint numExecutedMacroLinesPtr = nint.Zero;
    public byte NumExecutedMacroLines
    {
        get => *(byte*)numExecutedMacroLinesPtr;
        set
        {
            if (numExecutedMacroLinesPtr != nint.Zero)
                SafeMemory.WriteBytes(numExecutedMacroLinesPtr, new[] { value });
        }
    }
    private MacroService()
    {
        DalamudApi.GameInteropProvider.InitializeFromAttributes(this);
        ExecuteMacroHook = DalamudApi.GameInteropProvider.HookFromSignature<ExecuteMacroDelegate>(macroSig, ExecuteMacroDetour);
        numCopiedMacroLinesPtr = DalamudApi.SigScanner.ScanText("49 8D 5E 70 BF ?? 00 00 00") + 0x5;
        numExecutedMacroLinesPtr = DalamudApi.SigScanner.ScanText("41 83 F8 ?? 0F 8D ?? ?? ?? ?? 49 6B C8 68") + 0x3;

        ExecuteMacroHook!.Enable();
    }
    public void ExecuteMacro(int id)
    {
        if (id is >= 0 and < 200)
        {
            ExecuteMacroHook!.Original(UIService.Instance.raptureShellModule, (nint)UIService.Instance.raptureMacroModule + 0x58 + (Macro.size * id));
        }
    }
    private void ExecuteMacroDetour(RaptureShellModule* raptureShellModule, nint macro)
    {
        NumCopiedMacroLines = Macro.numLines;
        NumExecutedMacroLines = Macro.numLines;
        ExecuteMacroHook!.Original(raptureShellModule, macro);
    }
    public void Dispose()
    {
        NumCopiedMacroLines = 15;
        NumExecutedMacroLines = 15;
        ExecuteMacroHook?.Dispose();
    }
}