using Assets.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TestLobby : MonoBehaviour
{
    public static TestLobby I => instance;
    static TestLobby instance;

    //private Lobby hostLobby;
    private Lobby joinedLobby;
    //private float heartbeatTimerMax = 15;
    //private float heartbeatTimer;
    private float lobbyPollTimerMax = 1.1f;
    private float lobbyPollTimer;

    private float quickjoinRetryTimerMax = 5f;
    private float quickjoinRetryTimer;

    private float relayRetryTimerMax = 5f;
    private float relayRetryTimer;

    private ILobbyEvents lobbyEvents;
    private bool retryLobbyQuickJoin = false;
    private bool waitingForRelayKey = false;
    private bool originalKeyWasNull = false;

    public event EventHandler<LobbyEventArgs> OnLobbyUpdate;
    public event EventHandler OnFinishedConnecting;

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        quickjoinRetryTimer = quickjoinRetryTimerMax;
        relayRetryTimer = relayRetryTimerMax;
    }

    public async Task<bool> AuthenticateClient()
    {
        var options = new InitializationOptions();
        var profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                //NetworkManagerUI.I.WriteLineToOutput("Signed in " + AuthenticationService.Instance.PlayerId);
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        return true;
    }

    private void Update()
    {
        //HandleLobbyHeartBeat();
        PotentiallyTryToJoinLobby();
        HandleLobbyPolling();
    }

    private async void PotentiallyTryToJoinLobby()
    {
        if (joinedLobby == null && retryLobbyQuickJoin)
        {
            quickjoinRetryTimer -= Time.deltaTime;
            if (quickjoinRetryTimer < 0f)
            {
                quickjoinRetryTimer = quickjoinRetryTimerMax;

                await JoinLobbyAndRelay();
            }
        }
    }

    //private async void HandleLobbyHeartBeat()
    //{
    //    if (hostLobby != null)
    //    {
    //        heartbeatTimer -= Time.deltaTime;
    //        if (heartbeatTimer < 0f)
    //        {
    //            heartbeatTimer = heartbeatTimerMax;
    //            NetworkManagerUI.I.WriteLineToOutput("I wanna send heartbeat for lobby " + hostLobby.Id);
    //            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    //        }
    //    }
    //}
    private async void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
            if (waitingForRelayKey && originalKeyWasNull)
            {
                relayRetryTimer -= Time.deltaTime;
                if (relayRetryTimer < 0f)
                {
                    //NetworkManagerUI.I.WriteLineToOutput($"handle lobby polling: waitingForRelay {waitingForRelayKey} originalKeyWasNull {originalKeyWasNull}");
                    Debug.Log($"handle lobby polling: waitingForRelay {waitingForRelayKey} originalKeyWasNull {originalKeyWasNull}");
                    relayRetryTimer = relayRetryTimerMax;

                    //NetworkManagerUI.I.WriteLineToOutput("Gonna retry");
                    Debug.Log("Gonna retry");
                    await WaitForRelayJoin();
                }
            }
        }
    }

    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        //NetworkManagerUI.I.WriteLineToOutput("I'm in OnLobbyChanged on Client");
        Debug.Log("I'm in OnLobbyChanged on Client");
        if (changes.LobbyDeleted)
        {
            //NetworkManagerUI.I.WriteLineToOutput("Lobby was deleted");
            Debug.Log("Lobby was deleted");
            // we have a problem
        }
        else 
        {
            if (waitingForRelayKey)
            {
                if (changes.Data.Changed && changes.Data.Value.ContainsKey(ServerSideManager.RELAY_KEY))
                {
                    //NetworkManagerUI.I.WriteLineToOutput("Calling WaitForRelayJoin another time");
                    Debug.Log("Calling WaitForRelayJoin another time");
                    await WaitForRelayJoin();
                }
            }
        }
    }

    //public void StopLobbyHeartBeat()
    //{
    //    hostLobby = null;
    //}

    //public async void CreateLobby() {
    //    try
    //    {
    //        string lobbyName = "MyLobby";
    //        int maxPlayers = 4;
    //        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

    //        hostLobby = lobby;

    //        Debug.Log("Created lobby: " + lobby.Name + " for up to " + lobby.MaxPlayers + " players");
    //        NetworkManagerUI.I.WriteLineToOutput("Created lobby: " + lobby.Name + " for up to " + lobby.MaxPlayers + " players");
    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        NetworkManagerUI.I.WriteLineToOutput(e.ToString());
    //    }
    //}

    public async Task JoinLobbyAndRelay()
    {
        //NetworkManagerUI.I.WriteLineToOutput("inside JoinLobbyAndRelay");
        Debug.Log("inside JoinLobbyAndRelay");
        var lobbyJoined = await TryQuickJoinLobbyAsync();
        if (lobbyJoined)
        {
            Debug.Log("lobbyJoined was true");
            //NetworkManagerUI.I.WriteLineToOutput("lobbyJoinded was true");
            retryLobbyQuickJoin = false;
            await WaitForRelayJoin();
        }
        else
        {
            //NetworkManagerUI.I.WriteLineToOutput("lobbyJoinded was false");
            Debug.Log("lobbyJoined was false");
            retryLobbyQuickJoin = true;
        }
    }

    public async Task<bool> TryQuickJoinLobbyAsync()
    {
        try
        {
            //NetworkManagerUI.I.WriteLineToOutput("Attempting to quick join.");
            Debug.Log("Attempting to quick join.");
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            if (joinedLobby == null)
            {
                return false;
            }
            //NetworkManagerUI.I.WriteLineToOutput(joinedLobby.Id);
            Debug.Log(joinedLobby.Id);

            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);
                //NetworkManagerUI.I.WriteLineToOutput("lobby events should be subscribed " + lobbyEvents.ToString());
                Debug.Log("lobby events should be subscribed " + lobbyEvents.ToString());
                return true;
            }
            catch (LobbyServiceException ex)
            {
                //NetworkManagerUI.I.WriteLineToOutput(ex.ToString());
                Debug.Log(ex.ToString());
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{joinedLobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
                return false;
            }
        }
        catch (LobbyServiceException e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.Reason.ToString());
            Debug.Log(e.Reason.ToString());
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                //NetworkManagerUI.I.WriteLineToOutput("The server must be offline.");
                Debug.Log("The server must be offline.");
            }
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
            return false;
        }
    }

    public async Task WaitForRelayJoin()
    {
        try
        {
            var dataDic = joinedLobby?.Data;
            if (dataDic != null)
            {
                originalKeyWasNull = !(dataDic.TryGetValue(ServerSideManager.RELAY_KEY, out var originalRelayKey)
                    && originalRelayKey?.Value != null);
                //NetworkManagerUI.I.WriteLineToOutput("originalRelayKey: " + originalRelayKey?.Value);
                Debug.Log("originalRelayKey: " + originalRelayKey?.Value);

                if (!originalKeyWasNull)
                {
                    //NetworkManagerUI.I.WriteLineToOutput(originalRelayKey?.Value);
                    Debug.Log(originalRelayKey?.Value);
                    var result = await TestRelay.I.JoinRelayNewWay(originalRelayKey?.Value);
                    //NetworkManagerUI.I.WriteLineToOutput(result.ToString());
                    Debug.Log(result.ToString());
                    if (result)
                    {
                        waitingForRelayKey = false;
                        ConversationManager.I.HasAllNeededConnections = true;
                        return;
                    }
                }
            }

            waitingForRelayKey = true;
            UpdatePlayerOptions upo1 = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> {
                    { ServerSideManager.TRIGGER_CREATE_RELAY_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, null)}
                }
            }; 
            UpdatePlayerOptions upo2 = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> {
                    { ServerSideManager.TRIGGER_CREATE_RELAY_KEY, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "true")}
                }
            };
            joinedLobby = await Lobbies.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, upo1);
            joinedLobby = await Lobbies.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, upo2);
        }
        catch (Exception e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
        }
    }

    public async Task<bool> CheckForLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Found: " + queryResponse.Results.Count);
            //NetworkManagerUI.I.WriteLineToOutput("Found: " + queryResponse.Results.Count);

            foreach (var result in queryResponse.Results)
            {
                //NetworkManagerUI.I.WriteLineToOutput("Result: " + result.Name + " " + result.MaxPlayers);
                Debug.Log("Result: " + result.Name + " " + result.MaxPlayers);
            }

            return queryResponse.Results.Count > 0;
        }
        catch (LobbyServiceException e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
            return false;
        }
    }

    public async Task LeaveLobby() 
    {
        try
        {
            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            }
        }
        catch (LobbyServiceException e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
        }
    }

    public void StopAllActivity()
    {
        retryLobbyQuickJoin = false;
        waitingForRelayKey = false;
    }

    public async Task DisconectFromEverything()
    {
        StopAllActivity();
        await LeaveLobby();
        NetworkManager.Singleton.Shutdown();
    }
}
