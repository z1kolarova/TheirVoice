using Assets.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ClientSideManager : MonoBehaviour
{
    public static ClientSideManager I => instance;
    static ClientSideManager instance;

    [SerializeField] public string PrivateLobbyCode = "TESTCODE";

    [SerializeField] LobbyNotFoundModal lobbyNotFoundModal;

    private Lobby joinedLobby;
    private float lobbyPollTimerMax = 1.1f;
    private float lobbyPollTimer;

    private Task lobbyPollTask;
    private Task waitForRelayJoinTask;

    private float quickjoinRetryTimerMax = 5f;
    private float quickjoinRetryTimer;

    private float relayRetryTimerMax = 5f;
    private float relayRetryTimer;

    private ILobbyEvents lobbyEvents;
    private bool clientAuthenticated = false;
    private bool retryLobbyQuickJoin = false;
    private bool waitingForRelayKey = false;
    private bool originalKeyWasNull = false;

    [HideInInspector] public bool HasAllNeededConnections = false;

    public event EventHandler<LobbyEventArgs> OnLobbyUpdate;

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

    #region start
    private void Start()
    {
        quickjoinRetryTimer = quickjoinRetryTimerMax;
        relayRetryTimer = relayRetryTimerMax;

        EstablishNeededConnections();
    }

    private async void EstablishNeededConnections()
    {
        // 1) authenticate client - needed no matter what
        // 2) try quickjoining public lobby
        // 2a) no lobby available -> show message where code can be entered to join a private lobby or allow retry
        // 2b) quickjoin works
        // 3) a lobby has been joined, establish relay

        //WILL THIS RESULT IN PLAYER CAPSULE TRYING TO SPAWN INTO MAIN MENU???
        if (!clientAuthenticated)
        {
            clientAuthenticated = await ClientSideManager.I.AuthenticateClient();
        }
        if (clientAuthenticated)
        {
            await ClientSideManager.I.JoinPublicLobbyAndRelay();
        }
        else
        {
            Debug.LogError("Authentication failed miserably and we have a problem...");
        }
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
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        return true;
    }
    #endregion start

    #region update
    private void Update()
    {
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

                await JoinPublicLobbyAndRelay();
            }
        }
    }

    private void HandleLobbyPolling()
    {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;
                if (!lobbyPollTask.IsWaitingForCompletion())
                {
                    lobbyPollTask = PollLoby();
                }
            }
            if (waitingForRelayKey && originalKeyWasNull)
            {
                relayRetryTimer -= Time.deltaTime;
                if (relayRetryTimer < 0f)
                {
                    Debug.Log($"handle lobby polling: waitingForRelay {waitingForRelayKey} originalKeyWasNull {originalKeyWasNull}");
                    relayRetryTimer = relayRetryTimerMax;

                    if (!waitForRelayJoinTask.IsWaitingForCompletion())
                    {
                        waitForRelayJoinTask = WaitForRelayJoin();
                    }
                }
            }
        }
    }

    private async Task PollLoby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            OnLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.ToString());
        }
    }
    #endregion update

    #region joining
    public async Task JoinPublicLobbyAndRelay()
    {
        Debug.Log("inside JoinLobbyAndRelay");

        var lobbyJoined = await TryQuickJoinLobbyAsync();
        Debug.Log($"lobbyJoined was {lobbyJoined}");

        if (lobbyJoined)
        {
            retryLobbyQuickJoin = false;

            if (!waitForRelayJoinTask.IsWaitingForCompletion())
            {
                waitForRelayJoinTask = WaitForRelayJoin();
            }
            await waitForRelayJoinTask;
        }
        else
        {
            lobbyNotFoundModal.SetActive(true);
        }
    }

    public async Task JoinPrivateLobbyAndRelay(string lobbyCode)
    {
        var lobbyJoined = await TryJoinPrivateLobbyByCodeAsync(lobbyCode);
        if (lobbyJoined)
        {
            Debug.Log("joined private lobby");
            retryLobbyQuickJoin = false;

            if (!waitForRelayJoinTask.IsWaitingForCompletion())
            {
                waitForRelayJoinTask = WaitForRelayJoin();
            }
            await waitForRelayJoinTask;
        }
        else
        {
            Debug.Log("private lobby join didn't work out either");
            lobbyNotFoundModal.SetActive(true);
        }
    }
    public async Task<bool> TryQuickJoinLobbyAsync()
    {
        try
        {
            Debug.Log("Attempting to quick join.");
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            if (joinedLobby == null)
            {
                return false;
            }
            Debug.Log(joinedLobby.Id);

            return await SubscribeToLobbyCallbacksAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Reason.ToString());
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                Debug.Log("The server must be offline.");
            }
            Debug.Log(e.ToString());
            return false;
        }
    }
    public async Task<bool> TryJoinPrivateLobbyByCodeAsync(string lobbyCode)
    {
        try
        {
            Debug.Log($"Attempting to join private lobby by code: {lobbyCode}");
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            if (joinedLobby == null)
            {
                return false;
            }
            Debug.Log(joinedLobby.Id);

            return await SubscribeToLobbyCallbacksAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Reason.ToString());
            if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
            {
                Debug.Log("The server must be offline.");
            }
            Debug.Log(e.ToString());
            return false;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }
    public async Task<bool> CheckForLobbies()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Found: " + queryResponse.Results.Count);

            foreach (var result in queryResponse.Results)
            {
                Debug.Log("Result: " + result.Name + " " + result.MaxPlayers);
            }

            return queryResponse.Results.Count > 0;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }
    #endregion joining

    #region callbacks

    private async Task<bool> SubscribeToLobbyCallbacksAsync()
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        try
        {
            lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(joinedLobby.Id, callbacks);
            Debug.Log("lobby events should be subscribed " + lobbyEvents.ToString());
            return true;
        }
        catch (LobbyServiceException ex)
        {
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

    private async void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            Debug.LogError("OnLobbyChanged - Lobby was deleted - PROBLEM!!!");
            // we have a problem
        }
        else
        {
            changes.ApplyToLobby(joinedLobby);

            if (waitingForRelayKey)
            {
                if (changes.Data.Changed && changes.Data.Value.ContainsKey(ServerSideManager.RELAY_KEY))
                {
                    Debug.Log("Calling WaitForRelayJoin another time");
                    if (!waitForRelayJoinTask.IsWaitingForCompletion())
                    {
                        waitForRelayJoinTask = WaitForRelayJoin();
                    }
                    await waitForRelayJoinTask;
                }
            }
        }
    }
    
    #endregion callbacks

    #region relay
    public async Task WaitForRelayJoin()
    {
        try
        {
            var dataDic = joinedLobby?.Data;
            if (dataDic != null)
            {
                originalKeyWasNull = !(dataDic.TryGetValue(ServerSideManager.RELAY_KEY, out var originalRelayKey)
                    && originalRelayKey?.Value != null);
                Debug.Log("originalRelayKey: " + originalRelayKey?.Value);

                if (!originalKeyWasNull)
                {
                    Debug.Log(originalRelayKey?.Value);
                    var result = await RelayManager.I.JoinRelayNewWay(originalRelayKey?.Value);
                    Debug.Log(result.ToString());
                    if (result)
                    {
                        Debug.Log("HasAllNeededConnections is being set to true");
                        waitingForRelayKey = false;
                        ClientSideManager.I.HasAllNeededConnections = true;
                        MainMenuManager.I.ProceedToLanguageSelectionDialogue();
                        //InfoModal.I.Display("All is looking good".ToUpper(),
                        //    "The connection to server has been successfully established, so all online features (NPCs having personalities using AI, optional Speech-to-Text) should work.");
                        return;
                    }
                }
            }

            waitingForRelayKey = true;
            Debug.Log("About to call TriggerLobbyAction");
            await TriggerLobbyAction(ServerSideManager.TRIGGER_CREATE_RELAY_KEY);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
    #endregion relay

    #region leaving
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

    public async Task DisconnectAndCloseApp()
    {
        await DisconectFromEverything();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
    }
    #endregion leaving

    private async Task TriggerLobbyAction(string key, PlayerDataObject.VisibilityOptions pdoVo = PlayerDataObject.VisibilityOptions.Member)
    {
        UpdatePlayerOptions clearingUPO = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject> {
                { key, new PlayerDataObject(pdoVo, null) }
            }
        };
        UpdatePlayerOptions settingUPO = new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject> {
                { key, new PlayerDataObject(pdoVo, "true") }
            }
        };

        joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, clearingUPO);
        joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, settingUPO);
    }
}
