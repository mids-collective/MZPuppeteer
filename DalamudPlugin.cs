using Dalamud.Plugin;
using Plugin;

namespace MZPuppeteer;

public class MZPuppeter : IDalamudPlugin
{
    private ServiceInitializer serviceInitializer;
    public MZPuppeter(DalamudPluginInterface dpi)
    {
        DalamudApi.Initialize(dpi);
        serviceInitializer = new ServiceInitializer();
    }
    public void Dispose()
    {
        serviceInitializer.Dispose();
    }
}