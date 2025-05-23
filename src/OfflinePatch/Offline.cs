using System;
using System.Collections;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace OfflinePatch;

static class Offline
{
    public static bool Init()
    {
        try
        {
            var menuSplash_onWelcomeFailure = AccessTools.DeclaredMethod(
                typeof(Menu_Splash),
                nameof(Menu_Splash.onWelcomeFailure),
                [typeof(string)]
            );

            if (menuSplash_onWelcomeFailure is null)
            {
                Plugin.Log.LogError("Target method 'Menu_Splash.onWelcomeFailure' not found!");
                return false;
            }

            Plugin.hooks.Add(
                new Hook(menuSplash_onWelcomeFailure, Hook_Menu_Splash_onWelcomeFailure)
            );

            var menuConnecting_OnLogin = AccessTools.DeclaredMethod(
                typeof(Menu_Connecting),
                nameof(Menu_Connecting.OnLogin),
                []
            );

            if (menuConnecting_OnLogin is null)
            {
                Plugin.Log.LogError("Target method 'Menu_Connecting.OnLogin' not found!");
                return false;
            }

            Plugin.hooks.Add(new Hook(menuConnecting_OnLogin, Hook_Menu_Connecting_OnAwake));

            var clientInfo_OnStartClient = AccessTools.DeclaredMethod(
                typeof(ClientInfo),
                nameof(ClientInfo.OnStartClient),
                []
            );

            if (clientInfo_OnStartClient is null)
            {
                Plugin.Log.LogError("Target method 'ClientInfo.OnStartClient' not found!");
                return false;
            }

            // TODO: this is hacky fix and not a proper one!
            Plugin.hooks.Add(new Hook(clientInfo_OnStartClient, Hook_ClientInfo_OnStartClient));

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError(ex);
        }
        return false;
    }

    static void Hook_Menu_Splash_onWelcomeFailure(
        Action<Menu_Splash, string> orig,
        Menu_Splash self,
        string response
    )
    {
        orig(self, response);
        self.LoadGame();
    }

    static void Hook_Menu_Connecting_OnAwake(Action<Menu_Connecting> orig, Menu_Connecting self)
    {
        self.isTest = true;
        orig(self);
    }

    static void Hook_ClientInfo_OnStartClient(Action<ClientInfo> orig, ClientInfo self)
    {
        orig(self);
        self.StartCoroutine(WaitAndRespawn());
    }

    // The player spawns in a stuck state, replicate /stuck to get unstuck
    static IEnumerator WaitAndRespawn()
    {
        yield return new WaitForSeconds(1);
        Plugin.Log.LogInfo(
            $"[{nameof(WaitAndRespawn)}] Waited for 1 second, respawning to get unstuck."
        );

        var chatSystem = ChatSystem.instance;
        chatSystem.StartCoroutine(chatSystem.Respawn());
    }
}
