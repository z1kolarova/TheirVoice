using Assets.Classes;
using System;
using System.Collections.Generic;
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

    public event EventHandler<PlayerCountEventArgs> OnPlayerCountChanged;
    public event EventHandler<LobbyEventArgs> OnLobbyCreated;

    private int playersInLobbyCount = 0;

    private void Start()
    {
        instance = this;
        OnPlayerCountChanged += (object o, PlayerCountEventArgs e) =>
        {
            playersInLobbyCount = e.newTotalCount;
            NetworkManagerUI.I.UpdatePlayerCounter(e);
        };
        OnLobbyCreated += (object o, LobbyEventArgs e) =>
        {
            if (e.lobby.Players.Count > 0 && !(e.lobby.Data != null && e.lobby.Data.TryGetValue(RELAY_KEY, out var relKey) && relKey != null))
            {
                NetworkManagerUI.I.WriteLineToOutput($"very special condition was met");
                NetworkManagerUI.I.WriteLineToOutput(e.lobby.Data.ToString());
                StartRelayAndUpdateLobby();
            }
        };
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPolling();
    }

    private void ReactToValueChange(int previousValue, int newValue)
    {
        NetworkManagerUI.I.WriteLineToOutput($"The value changed from {previousValue} to {newValue} on the server.");
    }

    public async Task AuthenticateServer()
    {
        var options = new InitializationOptions();
        var profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);

        AuthenticationService.Instance.SignedIn += () =>
        {
            NetworkManagerUI.I.WriteLineToOutput("Server has signed in as" + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                NetworkManagerUI.I.WriteLineToOutput("heartbeat for lobby " + hostLobby.Id);
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public void StopLobbyHeartBeat()
    {
        hostLobby = null;
    }

    private async void HandleLobbyPolling()
    {
        if (hostLobby != null)
        {
            var originalPlayerCount = playersInLobbyCount;
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;

                hostLobby = await LobbyService.Instance.GetLobbyAsync(hostLobby.Id);

                if (playersInLobbyCount != hostLobby.MaxPlayers - hostLobby.AvailableSlots - 1)
                {
                    OnPlayerCountChanged?.Invoke(this, new PlayerCountEventArgs
                    {
                        originalCount = playersInLobbyCount,
                        newTotalCount = hostLobby.MaxPlayers - hostLobby.AvailableSlots - 1
                    });
                }
            }
        }
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        NetworkManagerUI.I.WriteLineToOutput("I'm in OnLobbyChanged on Server");
        if (changes.PlayerJoined.Value?.Count > 0)
        {
            NetworkManagerUI.I.WriteLineToOutput($"players joined: {changes.PlayerJoined.Value?.Count}");
        }

        if (changes.PlayerLeft.Value?.Count > 0)
        {
            NetworkManagerUI.I.WriteLineToOutput($"players left: {changes.PlayerLeft.Value?.Count}");
        }

        if (changes.PlayerData.Added || changes.PlayerData.Changed)
        {
            foreach (var item in changes.PlayerData.Value)
            {
                if (item.Value.ChangedData.Value.ContainsKey(TRIGGER_CREATE_RELAY_KEY) 
                    && item.Value.ChangedData.Value[TRIGGER_CREATE_RELAY_KEY].Value.Value == "true")
                {
                    StartRelayAndUpdateLobby();
                }
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            Player player = new Player(AuthenticationService.Instance.PlayerId);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
                Data = new Dictionary<string, DataObject> {
                    { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, null) }
                }
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            try
            {
                lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            }
            catch (LobbyServiceException ex)
            {
                NetworkManagerUI.I.WriteLineToOutput(ex.ToString());
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
            }

            hostLobby = lobby;

            OnLobbyCreated?.Invoke(this, new LobbyEventArgs { lobby = lobby });
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    public async void StartRelayAndUpdateLobby()
    {
        string relayCode = await TestRelay.I.CreateRelayNewWay();

        var updatedLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> {
                { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
        });

        hostLobby = updatedLobby;
    }
}
