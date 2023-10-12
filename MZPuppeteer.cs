using Dalamud.Plugin;
using MZPuppeteer.Attributes;
using MZPuppeteer.Services;

namespace MZPuppeteer;

public unsafe class MZPuppeter : IDalamudPlugin
{
    public static MZPuppeter? Instance;
    private List<IDisposable> ServiceList = new();
    public MZPuppeter(DalamudPluginInterface dpi)
    {
        Instance = this;
        DalamudApi.Initialize(dpi);
        
        ServiceList.Add(UIService.Instance);
        ServiceList.Add(MacroService.Instance);
        ServiceList.Add(CmdService.Instance);
        ServiceList.Add(ConfigService.Instance);
        ServiceList.Add(PuppetService.Instance);
        ServiceList.Add(new PluginCommandManager(this));
    }
    [Command("/puppeteer")]
    [HelpMessage("Show or hide plugin configuation")]
    private void ToggleConfig(string cmd, string args)
    {
        ConfigService.Instance.ToggleConfig();
    }
    public void Dispose()
    {
        foreach (var service in ServiceList)
        {
            service.Dispose();
        }
    }
}