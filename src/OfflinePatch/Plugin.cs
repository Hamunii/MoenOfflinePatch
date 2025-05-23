using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace OfflinePatch;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static Plugin instance = null!;
    internal static List<IDetour> hooks = [];
    internal static Material slopeMaterial = null!;
    internal static Shader slopeShader = null!;
    internal static AssetBundle assets = null!;

    private void Awake()
    {
        instance = this;
        Log = base.Logger;

        if (Offline.Init())
        {
            Log.LogInfo($"{MyPluginInfo.PLUGIN_GUID} has loaded successfully!");
        }
        else
        {
            Log.LogError($"{MyPluginInfo.PLUGIN_GUID} failed to load!");
        }
    }
}
