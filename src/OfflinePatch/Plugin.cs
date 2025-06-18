using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using MonoDetour;

namespace OfflinePatch;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        try
        {
            MonoDetourManager.InvokeHookInitializers(Assembly.GetExecutingAssembly());
        }
        catch (Exception ex)
        {
            Log.LogError(ex);
            Log.LogError($"{MyPluginInfo.PLUGIN_GUID} failed to load!");
            return;
        }

        Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} has loaded successfully!");
    }
}
