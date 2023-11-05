using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;

namespace Plugin.Services;
public unsafe sealed class UIService : IService<UIService>
{
    public static UIService Instance => Service<UIService>.Instance;
    public static UIModule* uiModule => Framework.Instance()->GetUiModule();
    public static RaptureShellModule* raptureShellModule => uiModule->GetRaptureShellModule();
    public static RaptureMacroModule* raptureMacroModule => uiModule->GetRaptureMacroModule();
    private UIService()
    {
    }
    public void Dispose()
    {
    }
}