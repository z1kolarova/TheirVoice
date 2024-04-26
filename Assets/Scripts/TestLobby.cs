using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    public static TestLobby I => instance;
    static TestLobby instance;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimerMax = 15;
    private float heartbeatTimer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

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
                NetworkManagerUI.I.WriteLineToOutput("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        return true;
    }

    private void Update()
    {
        //HandleLobbyHeartBeat();
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

    public async void CreateLobby() {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            Debug.Log("Created lobby: " + lobby.Name + " for up to " + lobby.MaxPlayers + " players");
            NetworkManagerUI.I.WriteLineToOutput("Created lobby: " + lobby.Name + " for up to " + lobby.MaxPlayers + " players");
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    public async Task QuickJoinLobby()
    {
        try
        {
            NetworkManagerUI.I.WriteLineToOutput("Attempting to quick join.");
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            NetworkManagerUI.I.WriteLineToOutput(joinedLobby.Id);

            if (joinedLobby.Data[ServerSideManager.RELAY_KEY].Value != null)
            {
                NetworkManagerUI.I.WriteLineToOutput(joinedLobby.Data[ServerSideManager.RELAY_KEY].Value);
                var result = await TestRelay.I.JoinRelayNewWay(joinedLobby.Data[ServerSideManager.RELAY_KEY].Value);
                NetworkManagerUI.I.WriteLineToOutput(result.ToString());
            }
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                NetworkManagerUI.I.WriteLineToOutput("The server must be offline.");
            }
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    public async Task CheckForLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            NetworkManagerUI.I.WriteLineToOutput("Found: " + queryResponse.Results.Count);

            foreach (var result in queryResponse.Results)
            {
                NetworkManagerUI.I.WriteLineToOutput("Result: " + result.Name + " " + result.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    public async void LeaveLobby() 
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }
}
