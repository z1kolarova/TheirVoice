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

    private bool isReplacingLobby;

    public const string RELAY_KEY = "RelayKey";
    public const string TRIGGER_CREATE_RELAY_KEY = "PlsStartRelay";

    private ILobbyEvents lobbyEvents;

    private bool currentLobbyIsPrivate = false;

    public event Action OnLobbyFreshlyCreated;
    public event Action<int> OnPlayerJoined;
    public event Action<int> OnPlayerLeft;
    public event Action<int> OnLobbyRefreshed;

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
        await I.CreateLobby(lobbyName, 50, privateLobby);
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

        if (lobbyEvents != null)
        {
            _ = UnsubscribeQuietly(lobbyEvents);
            lobbyEvents = null;
        }

        if (hostLobby != null)
        {
            _ = DeleteLobbySafely(hostLobby.Id);
            hostLobby = null;
        }

        ServerSideManagerUI.I.UpdateDisplayedLobbyCode("");
    }

    private async Task UnsubscribeQuietly(ILobbyEvents events)
    {
        try { await events.UnsubscribeAsync(); }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("Unsubscribe old lobby events failed: " + e.Message);
        }
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

        if (I.CurrentPlayerCount() > 0 && !(hostLobby.Data != null && hostLobby.Data.TryGetValue(RELAY_KEY, out var relKey) && relKey != null))
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
            _ = I.ReplaceNonfunctionalLobby("re-created lobby", 50, currentLobbyIsPrivate);
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
        switch (state)
        {
            case LobbyEventConnectionState.Error:
                ServerSideManagerUI.I.WriteBadLineToOutput("lobby connection state changed to: " + state.ToString());
                ServerSideManagerUI.I.WriteYellowLineToOutput("Authentication Service is signed in: " + AuthenticationService.Instance.IsSignedIn.ToString());
                _ = I.ReplaceNonfunctionalLobby("lobby after error state", 50, currentLobbyIsPrivate);
                break;
            case LobbyEventConnectionState.Unsynced:
                ServerSideManagerUI.I.WriteBadLineToOutput("lobby connection state changed to: " + state.ToString());
                ServerSideManagerUI.I.WriteYellowLineToOutput("Authentication Service is signed in: " + AuthenticationService.Instance.IsSignedIn.ToString());
                _ = I.ReplaceNonfunctionalLobby("new lobby over debugging", 50, currentLobbyIsPrivate);
                break;
            case LobbyEventConnectionState.Subscribed:
                I.ReconcileLobbyState();
                break;
            ////case LobbyEventConnectionState.Unknown:
            ////case LobbyEventConnectionState.Unsubscribed:
            ////case LobbyEventConnectionState.Subscribing:
            default:
                ServerSideManagerUI.I.WriteLineToOutput("lobby connection state changed to: " + state.ToString());
                break;
        }
    }
    private void HandleBeingKickedFromLobby()
    {
        ServerSideManagerUI.I.WriteBadLineToOutput("somehow the server got kicked from lobby");
        _ = I.ReplaceNonfunctionalLobby("got kicked so new lobby", 50, currentLobbyIsPrivate);
    }
    private async Task ReplaceNonfunctionalLobby(string newLobbyName, int newMaxPlayers, bool privateLobby = false)
    {
        if (isReplacingLobby)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("already trying to replace lobby -> ignoring " + newLobbyName);
            return;
        }

        isReplacingLobby = true;
        try
        {
            ServerSideManagerUI.I.EmptyOutput();
            I.ShutDownHostLobby();
            await I.CreateLobby(newLobbyName, newMaxPlayers, privateLobby);
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("Replacing non-functional lobbby failed: " + e.Message);
        }
        finally 
        {
            isReplacingLobby = false;
        }
    }

    //private async void RestoreUnsyncedLobby()
    //{
    //    try
    //    {
    //        hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);
    //        I.ReconcileLobbyState();
    //        ServerSideManagerUI.I.WriteYellowLineToOutput("hostLobby last updated (b): " + hostLobby.LastUpdated.ToString("dd.MM.yyyy HH:mm"));
    //    }
    //    catch (LobbyServiceException ex) when (ex.Reason == LobbyExceptionReason.LobbyNotFound)
    //    {
    //        _ = I.ReplaceNonfunctionalLobby("lobby gone during unsynced", 50, currentLobbyIsPrivate);
    //    }
    //    catch (LobbyServiceException ex)
    //    {
    //        ServerSideManagerUI.I.WriteBadLineToOutput("RestoreUnsynced failed: " + ex);
    //        _ = I.ReplaceNonfunctionalLobby("lobby replacing old perma-unsynced", 50, currentLobbyIsPrivate);
    //    }
    //    catch (Exception e)
    //    {
    //        ServerSideManagerUI.I.WriteBadLineToOutput("different reason why resync failed: " + e.Message);
    //        ServerSideManagerUI.I.WriteBadLineToOutput("different reason why resync failed: " + e.StackTrace);
    //        ServerSideManagerUI.I.WriteBadLineToOutput("different reason why resync failed: " + e.Data);
    //    }
    //}

    private void ReconcileLobbyState()
    {
        if (hostLobby == null)
            return;

        OnLobbyRefreshed?.Invoke(I.CurrentPlayerCount());
        ServerSideManagerUI.I.WriteYellowLineToOutput("hostLobby last updated (a): " + hostLobby.LastUpdated.ToString("dd.MM.yyyy HH:mm"));
        ServerSideManagerUI.I.WriteYellowLineToOutput("ContainsKey RELAY_KEY: " + hostLobby.Data.ContainsKey(RELAY_KEY).ToString());
        ServerSideManagerUI.I.WriteYellowLineToOutput("[RELAY_KEY]: " + hostLobby.Data?[RELAY_KEY]?.Value.ToString());
        if (hostLobby.Data?[RELAY_KEY]?.Value == null
            && hostLobby.Players.Any(p 
                => p.Data.ContainsKey(TRIGGER_CREATE_RELAY_KEY) 
                && p.Data.TryGetValue(TRIGGER_CREATE_RELAY_KEY, out var v) 
                && v?.Value == "true")
            && !RelayManager.I.IsRelayCreationInProgress())
        {
            I.StartRelayAndUpdateLobby();
        }
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

    public async Task CreateLobby(string lobbyName, int maxPlayers, bool privateLobby = false)
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

            // On a fatal subscription failure stop here.
            // The lobby has been cleaned up (or the failure is unrecoverable).
            if (!await TrySubscribeToLobbyEvents(lobby))
            {
                await DeleteLobbySafely(lobby.Id);
                return;
            }

            I.InitHostLobby(lobby);
            OnLobbyFreshlyCreated?.Invoke();
            ServerSideManagerUI.I.WriteLineToOutput("End of CreateLobby, name: " + lobbyName + ", success: " + (hostLobby != null));
        }
        catch (LobbyServiceException e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("NOT LobbyServiceException: " + e.Message);
            ServerSideManagerUI.I.WriteBadLineToOutput(e.StackTrace);
            ServerSideManagerUI.I.WriteBadLineToOutput(e.InnerException?.Message ?? "no inner exception");
        }
    }

    /// <returns>true if subscribing to lobby event worked; false if it failed fatally.</returns>
    private async Task<bool> TrySubscribeToLobbyEvents(Lobby lobby)
    {
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
            return true;
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby:
                    Debug.LogWarning($"Already subscribed to lobby[{lobby.Id}]. {ex.Message}");
                    ServerSideManagerUI.I.WriteBadLineToOutput($"Already subscribed to lobby[{lobby.Id}]. {ex.Message}");
                    return true;

                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                    Debug.LogWarning($"Subscription to lobby[{lobby.Id}] lost while busy. {ex.Message}");
                    ServerSideManagerUI.I.WriteBadLineToOutput($"Subscription to lobby[{lobby.Id}] lost while busy. {ex.Message}");
                    return false;

                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    Debug.LogError($"Failed to connect to lobby events. {ex.Message}");
                    ServerSideManagerUI.I.WriteBadLineToOutput($"Failed to connect to lobby events. {ex.Message}");
                    return false;

                default:
                    Debug.LogError(ex.ToString());
                    ServerSideManagerUI.I.WriteBadLineToOutput(ex.ToString());
                    return false;
            }
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput("NOT LobbyServiceException: " + e.Message);
            ServerSideManagerUI.I.WriteBadLineToOutput(e.StackTrace);
            ServerSideManagerUI.I.WriteBadLineToOutput(e.InnerException?.Message ?? "no inner exception");
            return false;
        }
    }

    private async Task DeleteLobbySafely(string lobbyId)
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            ServerSideManagerUI.I.WriteLineToOutputWithColor($"Successfully deleted lobby[{lobbyId}]", Color.white);
        }
        catch (LobbyServiceException ex)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput($"Failed to delete lobby[{lobbyId}]: {ex.Message}");
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput($"NOT LobbyServiceException while deleting lobby[{lobbyId}]: {e.Message}");
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
