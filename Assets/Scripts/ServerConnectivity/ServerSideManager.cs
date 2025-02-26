using Assets.Classes;
using System;
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
    private float heartbeatTimerMax = 20;
    private float heartbeatTimer;
    private float lobbyPollTimerMax = 1.1f;
    private float lobbyPollTimer;

    public const string RELAY_KEY = "RelayKey";
    public const string TRIGGER_CREATE_RELAY_KEY = "PlsStartRelay";

    private ILobbyEvents lobbyEvents;

    private int playersInLobbyCount = 0;

    private bool lobbyFreshlyCreated = false;
    private bool currentLobbyIsPrivate = false;
    private bool lobbyPlayerCountChanged = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPolling();
        HandlePseudoEvents();
    }

    public async Task StartServer(bool privateLobby)
    {
        await AuthenticateServer();

        var lobbyName = $"initial{(privateLobby ? "Private" : "Public")}Lobby";
        CreateLobby(lobbyName, 50, privateLobby);
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

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                ServerSideManagerUI.I.WriteLineToOutput("heartbeat for lobby " + hostLobby.Id);
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    ServerSideManagerUI.I.WriteLineToOutput($"{e.Reason}: {e.Message}");
                }
                catch (Exception e)
                {
                    ServerSideManagerUI.I.WriteLineToOutput(e.ToString());
                }
            }
        }
    }

    public void StopLobbyHeartBeat()
    {        
        hostLobby = null;
        ServerSideManagerUI.I.UpdateDisplayedLobbyCode("");
    }

    private async void HandleLobbyPolling()
    {
        if (hostLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;

                hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);

                
                if (playersInLobbyCount != hostLobby.MaxPlayers - hostLobby.AvailableSlots - 1)
                {
                    lobbyPlayerCountChanged = true;
                }
            }
        }
    }

    private void HandlePseudoEvents()
    {
        if (lobbyFreshlyCreated)
        {
            lobbyFreshlyCreated = false;
            ServerSideManagerUI.I.WriteLineToOutput("Handling freshly created lobby");

            if (hostLobby.IsPrivate)
            {
                ServerSideManagerUI.I.UpdateDisplayedLobbyCode(hostLobby.LobbyCode);
            }

            if (hostLobby.Players.Count > 0 && !(hostLobby.Data != null && hostLobby.Data.TryGetValue(RELAY_KEY, out var relKey) && relKey != null))
            {
                ServerSideManagerUI.I.WriteLineToOutput($"lobby is freshly created, at least 1 player is already present but relay key is not available");
                ServerSideManagerUI.I.WriteLineToOutput(hostLobby.Data.ToString());
                StartRelayAndUpdateLobby();
            }
        }
        if (lobbyPlayerCountChanged)
        {
            ServerSideManagerUI.I.WriteLineToOutput($"playercountchanged");
            lobbyPlayerCountChanged = false;

            var pcea = new PlayerCountEventArgs { 
                originalCount = playersInLobbyCount,
                newTotalCount = hostLobby.MaxPlayers - hostLobby.AvailableSlots - 1
            };

            playersInLobbyCount = pcea.newTotalCount;
            ServerSideManagerUI.I.UpdatePlayerCounter(pcea);
        }
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        ServerSideManagerUI.I.WriteLineToOutput("I'm in OnLobbyChanged on Server");

        if (changes.LobbyDeleted)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput($"lobby got deleted");
            ReplaceNonfunctionalLobby("re-created lobby", 50, currentLobbyIsPrivate);
        }
        else
        {

            if (changes.PlayerJoined.Value?.Count > 0)
            {
                ServerSideManagerUI.I.WriteLineToOutput($"players joined: {changes.PlayerJoined.Value?.Count}");
            }

            if (changes.PlayerLeft.Value?.Count > 0)
            {
                ServerSideManagerUI.I.WriteLineToOutput($"players left: {changes.PlayerLeft.Value?.Count}");
            }

            if (changes.PlayerData.Added || changes.PlayerData.Changed)
            {
                if (changes.PlayerData.Value.Any(
                    item => item.Value.ChangedData.Value.ContainsKey(TRIGGER_CREATE_RELAY_KEY)
                        && item.Value.ChangedData.Value[TRIGGER_CREATE_RELAY_KEY].Value.Value == "true"))
                {
                    foreach (var dat in changes.PlayerData.Value.FirstOrDefault().Value.ChangedData.Value)
                    {
                        ServerSideManagerUI.I.WriteLineToOutput($"key: {dat.Key}, value: {dat.Value.Value.Value}");
                    }

                    //NetworkManagerUI.I.WriteLineToOutput(changes.PlayerData.Value.FirstOrDefault().Value.LastUpdatedChanged.Value.ToString());
                    StartRelayAndUpdateLobby();
                }
            }
        }
    }

    private void OnConnectionStateChanged(LobbyEventConnectionState state)
    {
        ServerSideManagerUI.I.WriteLineToOutput("lobby connection state changed to: " + state.ToString());
        if (state == LobbyEventConnectionState.Error)
        {
            ReplaceNonfunctionalLobby("lobby after error state", 50, currentLobbyIsPrivate);
        }
    }
    private void OnKickedFromLobby()
    {
        ServerSideManagerUI.I.WriteBadLineToOutput("somehow the server got kicked from lobby");
        ReplaceNonfunctionalLobby("got kicked so new lobby", 50, currentLobbyIsPrivate);
    }

    private void ReplaceNonfunctionalLobby(string newLobbyName, int newMaxPlayers, bool privateLobby = false)
    {
        ServerSideManagerUI.I.EmptyOutput();
        StopLobbyHeartBeat();
        CreateLobby(newLobbyName, newMaxPlayers, privateLobby);
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

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            callbacks.LobbyEventConnectionStateChanged += OnConnectionStateChanged;
            callbacks.KickedFromLobby += OnKickedFromLobby;
            try
            {
                lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
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

            lobbyFreshlyCreated = true;
            hostLobby = lobby;
            currentLobbyIsPrivate = hostLobby.IsPrivate;
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

        var updatedLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
        });

        hostLobby = updatedLobby;
    }
}
