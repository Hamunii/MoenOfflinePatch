using System.Collections;
using MonoDetour;
using UnityEngine;

namespace OfflinePatch;

[MonoDetourTargets(typeof(Menu_Splash))]
[MonoDetourTargets(typeof(Menu_Connecting))]
[MonoDetourTargets(typeof(ClientInfo))]
static class Offline
{
    [MonoDetourHookInit]
    public static void Init()
    {
        On.Menu_Splash.onWelcomeFailure.Postfix(Postfix_onWelcomeFailure);
        On.Menu_Connecting.OnLogin.Prefix(Prefix_OnLogin);

        // TODO: this is hacky fix and not a proper one!
        On.ClientInfo.OnStartClient.Postfix(Postfix_OnStartClient);
    }

    static void Prefix_OnLogin(Menu_Connecting self)
    {
        self.isTest = true;
    }

    static void Postfix_onWelcomeFailure(Menu_Splash self, ref string response)
    {
        self.LoadGame();
    }

    static void Postfix_OnStartClient(ClientInfo self)
    {
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
