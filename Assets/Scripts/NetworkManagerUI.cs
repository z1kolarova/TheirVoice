using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
        outputTMP.text = "";
        outputTMP.text += "I am awake\n";
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
            NetworkManager.Singleton.Shutdown();
            outputTMP.text += "NetworkManager.Singleton.Shutdown happened\n";
            serverBtn.gameObject.SetActive(true);
            serverBtn.enabled = true;
            clientBtn.gameObject.SetActive(true);
            clientBtn.enabled = true;
            shutdownBtn.enabled = false;

            if (actsAsARunningServer)
            {
                TestLobby.I.StopLobbyHeartBeat();
                ServerSideManager.I.StopLobbyHeartBeat();
                actsAsARunningServer = false;
            }
        });

        outputTMP.text += "I started\n";
        outputTMP.text += TestLobby.I != null;
    }

    public void WriteLineToOutput(string text)
    {
        outputTMP.text += text + "\n";
    }


    private async void ServerStartProcess()
    {
        await ServerSideManager.I.AuthenticateServer();
        ServerSideManager.I.CreateLobby("testingLobby", 5);
        //TestLobby.I.CreateLobby();
    }

    private async void ClientStartProcess()
    {
        var authenticated = await TestLobby.I.AuthenticateClient();
        if (authenticated)
        {
            await TestLobby.I.CheckForLobbies();
            await TestLobby.I.QuickJoinLobby();
        }
        else
        {
            NetworkManagerUI.I.WriteLineToOutput("Authentication failed miserably and we have a problem...");
        }
    }
}
