using Assets.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
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

    private bool lobbyFreshlyCreated = false;
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

    public async Task AuthenticateServer()
    {
        var options = new InitializationOptions();
        var profile = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profile);

        await UnityServices.InitializeAsync(options);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                NetworkManagerUI.I.WriteLineToOutput("Server has signed in as" + AuthenticationService.Instance.PlayerId);
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
                NetworkManagerUI.I.WriteLineToOutput("heartbeat for lobby " + hostLobby.Id);
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    NetworkManagerUI.I.WriteLineToOutput($"{e.Reason}: {e.Message}");
                }
                catch (Exception e)
                {
                    NetworkManagerUI.I.WriteLineToOutput(e.ToString());
                }
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
            if (hostLobby.Players.Count > 0 && !(hostLobby.Data != null && hostLobby.Data.TryGetValue(RELAY_KEY, out var relKey) && relKey != null))
            {
                NetworkManagerUI.I.WriteLineToOutput($"very special condition was met");
                NetworkManagerUI.I.WriteLineToOutput(hostLobby.Data.ToString());
                StartRelayAndUpdateLobby();
            }
        }
        if (lobbyPlayerCountChanged)
        {
            NetworkManagerUI.I.WriteLineToOutput($"playercountchanged");
            lobbyPlayerCountChanged = false;

            var pcea = new PlayerCountEventArgs { 
                originalCount = playersInLobbyCount,
                newTotalCount = hostLobby.MaxPlayers - hostLobby.AvailableSlots - 1
            };

            playersInLobbyCount = pcea.newTotalCount;
            NetworkManagerUI.I.UpdatePlayerCounter(pcea);
        }
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        NetworkManagerUI.I.WriteLineToOutput("I'm in OnLobbyChanged on Server");

        Debug.Log("I'm in OnLobbyChanged on Client");
        if (changes.LobbyDeleted)
        {
            NetworkManagerUI.I.WriteBadLineToOutput($"lobby got deleted");
            hostLobby = null;
            CreateLobby("re-created lobby", 100);
        }
        else
        {

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
                if (changes.PlayerData.Value.Any(
                    item => item.Value.ChangedData.Value.ContainsKey(TRIGGER_CREATE_RELAY_KEY)
                        && item.Value.ChangedData.Value[TRIGGER_CREATE_RELAY_KEY].Value.Value == "true"))
                {
                    foreach (var dat in changes.PlayerData.Value.FirstOrDefault().Value.ChangedData.Value)
                    {
                        NetworkManagerUI.I.WriteLineToOutput($"key: {dat.Key}, value: {dat.Value.Value.Value}");
                    }

                    //NetworkManagerUI.I.WriteLineToOutput(changes.PlayerData.Value.FirstOrDefault().Value.LastUpdatedChanged.Value.ToString());
                    StartRelayAndUpdateLobby();
                }
            }
        }
    }

    private void OnConnectionStateChanged(LobbyEventConnectionState state)
    {
        NetworkManagerUI.I.WriteLineToOutput("lobby connection state changed to: " + state.ToString());
    }
    private void OnKickedFromLobby()
    {
        NetworkManagerUI.I.WriteBadLineToOutput("somehow the server got kicked from lobby");
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            NetworkManagerUI.I.WriteLineToOutput("In CreateLobby");
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
            callbacks.LobbyEventConnectionStateChanged += OnConnectionStateChanged;
            callbacks.KickedFromLobby += OnKickedFromLobby;
            try
            {
                lobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
            }
            catch (LobbyServiceException ex)
            {
                NetworkManagerUI.I.WriteBadLineToOutput(ex.ToString());
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                    case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                    case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                    default: throw;
                }
            }

            lobbyFreshlyCreated = true;
            hostLobby = lobby;
            NetworkManagerUI.I.WriteLineToOutput("End of CreateLobby " + (hostLobby != null).ToString());
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteBadLineToOutput(e.ToString());
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
