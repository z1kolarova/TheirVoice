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

public class ServerSideManager : MonoBehaviour
{
    public static ServerSideManager I => instance;
    static ServerSideManager instance;

    private Lobby hostLobby;
    private float heartbeatTimerMax = 20;
    private float heartbeatTimer;
    private float lobbyPollTimer;
    private float lobbyPollTimerMax = 1.1f;

    public const string RELAY_KEY = "RelayKey";

    public event EventHandler<LobbyEventArgs> OnCreatedLobby;
    public event EventHandler<LobbyEventArgs> OnPlayerJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnPlayerLeftLobby;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }

    private void Start()
    {
        instance = this;
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
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
                NetworkManagerUI.I.WriteLineToOutput("I wanna send heartbeat for lobby " + hostLobby.Id);
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public void StopLobbyHeartBeat()
    {
        hostLobby = null;
    }

    public async void CreateLobby(string lobbyName, int maxPlayers)
    {
        try
        {
            Player player = new Player(AuthenticationService.Instance.PlayerId);

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Player = player,
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            hostLobby = lobby;

            OnCreatedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby });

            string relayCode = await TestRelay.I.CreateRelayNewWay();

            var updatedLobby = await Lobbies.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { RELAY_KEY, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });

            hostLobby = updatedLobby;
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }
}
