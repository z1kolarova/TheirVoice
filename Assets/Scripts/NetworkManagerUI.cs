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
    public string logText = "";
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
            logText += "Server button was clicked\n";
            outputTMP.text = logText;
            ServerStartProcess();
            actsAsARunningServer = true;
            logText += "NetworkManager.Singleton.StartServer happened\n";
            outputTMP.text = logText;
            serverBtn.enabled = false;
            serverBtn.gameObject.SetActive(false);
            clientBtn.enabled = false;
            clientBtn.gameObject.SetActive(false);
            shutdownBtn.enabled = true;
        });

        clientBtn.onClick.AddListener(() => {
            logText += "Client button was clicked\n";
            outputTMP.text = logText;
            ClientStartProcess();
            logText += "NetworkManager.Singleton.StartClient happened\n";
            outputTMP.text = logText;
            serverBtn.enabled = false;
            clientBtn.enabled = false;
            clientBtn.gameObject.SetActive(false);
            shutdownBtn.enabled = true;
        });

        shutdownBtn.onClick.AddListener(() => {
            logText += "Shutdown button was clicked\n";
            outputTMP.text = logText;
            
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
            logText += "NetworkManager.Singleton.Shutdown happened\n";
            outputTMP.text = logText;
            serverBtn.gameObject.SetActive(true);
            serverBtn.enabled = true;
            clientBtn.gameObject.SetActive(true);
            clientBtn.enabled = true;
            shutdownBtn.enabled = false;
        });
    }

    public void WriteLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = $"{DateTime.Now.ToString("HH:mm:ss")}: {text}";
        logText += $"{lineContent}\n";
        outputTMP.text = logText;
    }
    
    public void WriteBadLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = $"{DateTime.Now.ToString("HH:mm:ss")} ERROR: {text}";
        logText += $"<color=#FF0000>{lineContent}\n</color>";
        outputTMP.text = logText;
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
        ServerSideManager.I.CreateLobby("testingLobby", 100);
        //TestLobby.I.CreateLobby();
    }

    private async void ClientStartProcess()
    {
        logText += "Inside ClientStartProcess\n";
        outputTMP.text = logText;
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
            NetworkManagerUI.I.WriteBadLineToOutput("Authentication failed miserably and we have a problem...");
        }
    }
}
