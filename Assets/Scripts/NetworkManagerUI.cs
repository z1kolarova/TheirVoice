using Assets.Classes;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    public static NetworkManagerUI I => instance;
    static NetworkManagerUI instance;

    [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button shutdownBtn;
    [SerializeField] private TMP_Text outputTMP;
    [SerializeField] private TMP_Text currentPlayersTMP;

    private bool actsAsARunningServer = false;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        serverBtn.onClick.AddListener(() => {
            outputTMP.text += "Server button was clicked\n";
            ServerStartProcess();
            actsAsARunningServer = true;
            outputTMP.text += "NetworkManager.Singleton.StartServer happened\n";
            serverBtn.enabled = false;
            serverBtn.gameObject.SetActive(false);
            clientBtn.enabled = false;
            clientBtn.gameObject.SetActive(false);
            shutdownBtn.enabled = true;
        });

        clientBtn.onClick.AddListener(() => {
            outputTMP.text += "Client button was clicked\n";
            ClientStartProcess();
            outputTMP.text += "NetworkManager.Singleton.StartClient happened\n";
            serverBtn.enabled = false;
            clientBtn.enabled = false;
            clientBtn.gameObject.SetActive(false);
            shutdownBtn.enabled = true;
        });

        shutdownBtn.onClick.AddListener(() => {
            outputTMP.text += "Shutdown button was clicked\n";

            if (actsAsARunningServer)
            {
                //TestLobby.I.StopLobbyHeartBeat();
                ServerSideManager.I.StopLobbyHeartBeat();
                actsAsARunningServer = false;
            }
            else {
                TestLobby.I.StopAllActivity();
                TestLobby.I.LeaveLobby();
            }

            NetworkManager.Singleton.Shutdown();
            outputTMP.text += "NetworkManager.Singleton.Shutdown happened\n";
            serverBtn.gameObject.SetActive(true);
            serverBtn.enabled = true;
            clientBtn.gameObject.SetActive(true);
            clientBtn.enabled = true;
            shutdownBtn.enabled = false;
        });
    }

    public void WriteLineToOutput(string text, bool timestamp = true)
    {
        outputTMP.text += $"{DateTime.Now.ToString("HH:mm:ss")}: {text}\n";
    }

    public void UpdatePlayerCounter(PlayerCountEventArgs e) 
    {
        WriteLineToOutput($"Changing player counter from {e.originalCount} to {e.newTotalCount}");
        if (e == null)
            return;

        currentPlayersTMP.text = e.newTotalCount.ToString();
    }


    private async void ServerStartProcess()
    {
        NetworkManagerUI.I.WriteLineToOutput("In ServerStartProcess");
        await ServerSideManager.I.AuthenticateServer();
        ServerSideManager.I.CreateLobby("testingLobby", 5);
        //TestLobby.I.CreateLobby();
    }

    private async void ClientStartProcess()
    {
        outputTMP.text += "Inside ClientStartProcess\n";
        var authenticated = await TestLobby.I.AuthenticateClient();
        NetworkManagerUI.I.WriteLineToOutput("authenticated " + authenticated);
        if (authenticated)
        {
            await TestLobby.I.JoinLobbyAndRelay();
            //await TestLobby.I.CheckForLobbies();
            //await TestLobby.I.TryQuickJoinLobby();
            //NetworkManagerUI.I.WriteLineToOutput("Calling WaitForRelayJoin the first time");
            //await TestLobby.I.WaitForRelayJoin();
        }
        else
        {
            NetworkManagerUI.I.WriteLineToOutput("Authentication failed miserably and we have a problem...");
        }
    }
}
