using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager I => instance;
    static LobbyManager instance;

    public event EventHandler OnLeftLobby;

    //public event EventHandler<LobbyEventArgs> OnJoinedLobby;
    public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;

    public class LobbyEventArgs : EventArgs {
        public Lobby lobby;
    }

    private float heartbeatTimer;
    private float heartbeatTimerMax = 20;
    private float lobbyPollTimer;
    private float lobbyPollTimerMax = 1.1f;

    private Lobby joinedLobby;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        //HandleLobbyHeartBeat();
        HandleLobbyPolling();
    }

    public async void Authenticate()
    {
        var options = new InitializationOptions();
        var profileName = Guid.NewGuid().ToString().Substring(0, 8);
        options.SetProfile(profileName);

        await UnityServices.InitializeAsync(options);

        heartbeatTimer = heartbeatTimerMax;

        AuthenticationService.Instance.SignedIn += () =>
        {
            //NetworkManagerUI.I.WriteLineToOutput("Signed in " + AuthenticationService.Instance.PlayerId);
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            //quick join?
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyHeartBeat() {
        if (IsLobbyHost()) {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                //NetworkManagerUI.I.WriteLineToOutput("I wanna send heartbeat for lobby " + joinedLobby.Id);
                Debug.Log("I wanna send heartbeat for lobby " + joinedLobby.Id);
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPolling() {
        if (joinedLobby != null)
        {
            lobbyPollTimer -= Time.deltaTime;
            if (lobbyPollTimer < 0f)
            {
                lobbyPollTimer = lobbyPollTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                OnJoinedLobbyUpdate?.Invoke(this, new LobbyEventArgs { lobby = joinedLobby });
            }
        }
    }

    public bool IsLobbyHost() { 
        return joinedLobby != null;
    }

    public async Task<bool> IsJoinable()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(options);
            return queryResponse.Results.Count > 0;
        }
        catch (LobbyServiceException e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
            return false;
        }
    }
}
