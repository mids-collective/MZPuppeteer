using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace MZPuppeteer
{
    public class DalamudApi
    {
        public static void Initialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<DalamudApi>();
        // @formatter:off
        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; }
        [PluginService] public static IPluginLog PluginLog { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static ICommandManager Commands { get; private set; }
        [PluginService] public static IChatGui Chat { get; private set; }
        [PluginService] public static ISigScanner SigScanner { get; private set; }
        // @formatter:on
    }
}