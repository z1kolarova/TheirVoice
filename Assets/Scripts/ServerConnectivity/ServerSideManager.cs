using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ServerSideManager : MonoBehaviour
{
    public static ServerSideManager I => instance;
    static ServerSideManager instance;

    private Lobby hostLobby;
    public int CurrentPlayerCount() 
        => hostLobby?.MaxPlayers - hostLobby?.AvailableSlots - 1 ?? 0;

    private float heartbeatSeconds = 20;
    private Coroutine heartbeatCoroutine;

    public const string RELAY_KEY = "RelayKey";
    public const string TRIGGER_CREATE_RELAY_KEY = "PlsStartRelay";

    private ILobbyEvents lobbyEvents;

    private bool currentLobbyIsPrivate = false;

    public event Action OnLobbyFreshlyCreated;
    public event Action<int> OnPlayerJoined;
    public event Action<int> OnPlayerLeft;

    # region moderation config

    private Meta moderationMeta { get; set; }
    private Meta GetModerationMeta()
    {
        if (moderationMeta == null)
        {
            moderationMeta = DBService.I.Find<Meta>(meta => meta.Key == Meta.ModerationOnKey)
                ?? new Meta() { Key = Meta.ModerationOnKey, Value = "1" };
        }
        return moderationMeta;
    }

    private bool? moderationOn { get; set; }
    public bool ModerationIsOn()
    {
        if (moderationOn == null)
        {
            moderationOn = GetModerationMeta().Value != "0";
        }
        return moderationOn.Value;
    }

    public void SetAndSaveModeration(bool isOn)
    {
        moderationOn = isOn;
        moderationMeta.Value = isOn ? "1" : "0";
        DBService.I.Upsert(moderationMeta);
    }

    #endregion moderation config

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        OnLobbyFreshlyCreated += HandleLobbyFreshlyCreated;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        OnLobbyFreshlyCreated -= HandleLobbyFreshlyCreated;
    }

    public async Task StartServer(bool privateLobby)
    {
        await I.AuthenticateServer();

        var lobbyName = $"initial{(privateLobby ? "Private" : "Public")}Lobby";
        I.CreateLobby(lobbyName, 50, privateLobby);
    }

    private async Task AuthenticateServer()
    {
        var options = new InitializationOptions();
        var profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                ServerSideManagerUI.I.WriteLineToOutput("Server has signed in as" + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void InitHostLobby(Lobby lobby)
    {
        hostLobby = lobby;
        currentLobbyIsPrivate = hostLobby.IsPrivate;
        heartbeatCoroutine = StartCoroutine(I.KeepHearbeat());
    }

    #region Coroutines
    private IEnumerator KeepHearbeat()
    {
        var wait = new WaitForSeconds(heartbeatSeconds);
        while (hostLobby != null)
        {
            ServerSideManagerUI.I.WriteLineToOutput("heartbeat for lobby " + hostLobby.Id);
            var task = LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            yield return new WaitUntil(() => task.IsCompleted); // wait for async to finish
            if (task.Exception != null)
            {
                ServerSideManagerUI.I.WriteLineToOutput(task.Exception.ToString());
            }
            yield return wait;
        }
    }
    #endregion Coroutines

    public void ShutDownHostLobby()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }

        hostLobby = null;
        ServerSideManagerUI.I.UpdateDisplayedLobbyCode("");
    }

    private void HandleLobbyFreshlyCreated()
    {
        ServerSideManagerUI.I.WriteLineToOutput("Handling freshly created lobby");

        if (hostLobby == null)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("Host lobby is null when it should be FRESHLY CREATED...");
            return;
        }

        if (hostLobby.IsPrivate)
        {
            ServerSideManagerUI.I.UpdateDisplayedLobbyCode(hostLobby.LobbyCode);
        }

        if (hostLobby.Players.Count > 0 && !(hostLobby.Data != null && hostLobby.Data.TryGetValue(RELAY_KEY, out var relKey) && relKey != null))
        {
            ServerSideManagerUI.I.WriteLineToOutput($"lobby is freshly created, at least 1 player is already present but relay key is not available");
            ServerSideManagerUI.I.WriteLineToOutput(hostLobby.Data.ToString());
            I.StartRelayAndUpdateLobby();
        }
    }

    private void HandleLobbyChanged(ILobbyChanges changes)
    {
        ServerSideManagerUI.I.WriteLineToOutput("I'm in OnLobbyChanged on Server");

        if (changes.LobbyDeleted)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput($"lobby got deleted");
            I.ReplaceNonfunctionalLobby("re-created lobby", 50, currentLobbyIsPrivate);
        }
        else
        {
            if (hostLobby != null)
            {
                changes.ApplyToLobby(hostLobby);
            }

            if (changes.PlayerData.Added || changes.PlayerData.Changed)
            {
                if (changes.PlayerData.Value.Any(
                    item => item.Value?.ChangedData.Value?.ContainsKey(TRIGGER_CREATE_RELAY_KEY) ?? false
                        && item.Value.ChangedData.Value[TRIGGER_CREATE_RELAY_KEY].Value.Value == "true"))
                {
                    foreach (var dat in changes.PlayerData.Value.FirstOrDefault().Value.ChangedData.Value)
                    {
                        ServerSideManagerUI.I.WriteLineToOutput($"key: {dat.Key}, value: {dat.Value.Value.Value}");
                    }

                    ServerSideManagerUI.I.WriteYellowLineToOutput("PlayerData.Added " + changes.PlayerData.Added);
                    ServerSideManagerUI.I.WriteYellowLineToOutput("PlayerData.Changed " + changes.PlayerData.Changed);
                    I.StartRelayAndUpdateLobby();
                }
            } else {
                ServerSideManagerUI.I.WriteLineToOutputWithColor($"no PlayerData added nor changed", color: Color.white);
            }
        }
    }

    private void HandlePlayerJoined(List<LobbyPlayerJoined> list) 
    {
        ServerSideManagerUI.I.WriteLineToOutput("PlayerJoined should be getting invoked");
        OnPlayerJoined?.Invoke(list.Count);
    }

    private void HandlePlayerLeft(List<int> list)
    {
        OnPlayerLeft?.Invoke(list.Count);
    }

    private void HandleConnectionStateChanged(LobbyEventConnectionState state)
    {
        ServerSideManagerUI.I.WriteLineToOutput("lobby connection state changed to: " + state.ToString());
        if (state == LobbyEventConnectionState.Error)
        {
            I.ReplaceNonfunctionalLobby("lobby after error state", 50, currentLobbyIsPrivate);
        }
    }
    private void HandleBeingKickedFromLobby()
    {
        ServerSideManagerUI.I.WriteBadLineToOutput("somehow the server got kicked from lobby");
        I.ReplaceNonfunctionalLobby("got kicked so new lobby", 50, currentLobbyIsPrivate);
    }
    private void ReplaceNonfunctionalLobby(string newLobbyName, int newMaxPlayers, bool privateLobby = false)
    {
        ServerSideManagerUI.I.EmptyOutput();
        I.ShutDownHostLobby();
        I.CreateLobby(newLobbyName, newMaxPlayers, privateLobby);
    }

    private void HandlePlayerDataAdded(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> pda)
    {
        I.PdaLog(pda, Color.orange);
    }
    private void HandlePlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> pda)
    {
        I.PdaLog(pda, Color.lightPink);
    }

    private void PdaLog(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> pda, Color c)
    {
        foreach (var key in pda.Keys)
        {
            foreach (var innerKey in pda[key].Keys)
            {
                ServerSideManagerUI.I.WriteLineToOutputWithColor(
                    "key: " + key + " innerKey: " + innerKey + " value: "
                    + pda[key][innerKey].Value?.Value?.ToString()
                    , c);
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool privateLobby = false)
    {
        try
        {
            ServerSideManagerUI.I.WriteLineToOutput("In CreateLobby");
            Player player = new Player(AuthenticationService.Instance.PlayerId);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = privateLobby,
                Data = new Dictionary<string, DataObject> {
                    { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, null) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += I.HandleLobbyChanged;
            callbacks.LobbyEventConnectionStateChanged += I.HandleConnectionStateChanged;
            callbacks.KickedFromLobby += I.HandleBeingKickedFromLobby;
            callbacks.PlayerJoined += I.HandlePlayerJoined;
            callbacks.PlayerLeft += I.HandlePlayerLeft;
            callbacks.PlayerDataAdded += I.HandlePlayerDataAdded;
            callbacks.PlayerDataChanged += I.HandlePlayerDataChanged;

            try
            {
                lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            }
            catch (LobbyServiceException ex)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(ex.ToString());
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby:
                        var msgAlreadySubscribedToLobby = $"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}";
                        Debug.LogWarning(msgAlreadySubscribedToLobby);
                        ServerSideManagerUI.I.WriteBadLineToOutput(msgAlreadySubscribedToLobby);
                        break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                        var msgSubscriptionToLobbyLostWhileBusy = $"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}";
                        Debug.LogWarning(msgSubscriptionToLobbyLostWhileBusy);
                        ServerSideManagerUI.I.WriteBadLineToOutput(msgSubscriptionToLobbyLostWhileBusy);
                        throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError:
                        var msgLobbyEventServiceConnectionError = $"Failed to connect to lobby events. Exception Message: {ex.Message}";
                        Debug.LogError(msgLobbyEventServiceConnectionError);
                        ServerSideManagerUI.I.WriteBadLineToOutput(msgLobbyEventServiceConnectionError);
                        throw;
                    default:
                        ServerSideManagerUI.I.WriteBadLineToOutput(ex.Message);
                        throw;
                }
            }

            I.InitHostLobby(lobby);
            OnLobbyFreshlyCreated?.Invoke();
            ServerSideManagerUI.I.WriteLineToOutput("End of CreateLobby, name: " + lobbyName + ", success: " + (hostLobby != null).ToString());
        }
        catch (LobbyServiceException e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
        }
    }

    public async void StartRelayAndUpdateLobby()
    {
        string relayCode = await RelayManager.I.CreateRelayNewWay();

        var updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
        });

        hostLobby = updatedLobby;
    }
}
