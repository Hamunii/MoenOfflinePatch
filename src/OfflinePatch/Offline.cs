using System;
using System.Collections;
using FishNet;
using MonoDetour;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoDetour.Reflection.Unspeakable;
using Steamworks;
using TMPro;
using UnityEngine;

namespace OfflinePatch;

[MonoDetourTargets(typeof(Menu_Splash))]
[MonoDetourTargets(typeof(Menu_Connecting), GenerateControlFlowVariants = true)]
[MonoDetourTargets(typeof(ClientInfo), GenerateControlFlowVariants = true)]
static class Offline
{
    static bool initialized = false;

    //? Used to tell the player that they are in offline mode
    private static TextMeshProUGUI? dummyTextElement;

    [MonoDetourHookInitialize]
    public static void Init()
    {
        On.Menu_Splash.onWelcomeFailure.Postfix(Postfix_Menu_Splash_onWelcomeFailure);
    }

    static void Postfix_Menu_Splash_onWelcomeFailure(Menu_Splash self, ref string response)
    {
        if (initialized)
            return;

        initialized = true;

        if (!SteamAPI.IsSteamRunning())
        {
            Plugin.Log.LogInfo("Steam is not running, logging in as Editor.");
            On.Menu_Connecting.OnLogin.Prefix(Prefix_Menu_Connecting_OnLogin);
        }
        else
        {
            Plugin.Log.LogInfo("Steam is running, logging in using Steam name.");
            On.Menu_Connecting.SendTicketToServer.ControlFlowPrefix(
                Prefix_Menu_Connecting_SendTicketToServer
            );
        }

        // TODO: this is hacky fix and not a proper one!
        On.ClientInfo.OnStartClient.Postfix(Postfix_ClientInfo_OnStartClient);

        On.ClientInfo.UpdatePlayerStatsPeriodically.ControlFlowPrefixMoveNext(
            Prefix_ClientInfo_UpdatePlayerStatsPeriodically_MoveNext
        );

        self.LoadGame();
    }

    private static ReturnFlow Prefix_Menu_Connecting_SendTicketToServer(
        Menu_Connecting self,
        ref string authTicket
    )
    {
        PlayerData.user = new UserData
        {
            displayName = SteamFriends.GetPersonaName(),
            steamID = "000000000",
            username = SteamFriends.GetPersonaName(),
        };

        InstanceFinder.NetworkManager.ClientManager.StartConnection();
        return ReturnFlow.SkipOriginal;
    }

    static void Prefix_Menu_Connecting_OnLogin(Menu_Connecting self)
    {
        self.isTest = true;
    }

    static void Postfix_ClientInfo_OnStartClient(ClientInfo self)
    {
        if (!self.IsOwner)
            return;

        self.StartCoroutine(WaitAndRespawn());

        dummyTextElement = GameObject
            .Find("AllCanvas/Menu_GameUI/Chat/ChatHolder/Image/CoinText")
            .GetComponent<TextMeshProUGUI>();
        dummyTextElement.text = "OFFLINE MODE";
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

    private static ReturnFlow Prefix_ClientInfo_UpdatePlayerStatsPeriodically_MoveNext(
        SpeakableEnumerator<object, ClientInfo> self,
        ref bool continueEnumeration
    )
    {
        // Cancel original from spamming network errors
        continueEnumeration = false;
        return ReturnFlow.SkipOriginal;
    }
}
