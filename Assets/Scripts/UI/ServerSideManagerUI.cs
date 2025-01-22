using Assets.Classes;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerSideManagerUI : MonoBehaviour
{
    public static ServerSideManagerUI I => instance;
    static ServerSideManagerUI instance;

    [Header("Buttons")]
    [SerializeField] private Button publicServerBtn;
    [SerializeField] private Button privateServerBtn;
    [SerializeField] private Button shutdownBtn;

    [SerializeField] private Button managePromptsBtn;

    [Header("Text outputs")]
    [SerializeField] private TMP_Text outputTMP;
    
    [SerializeField] private TMP_Text lobbyCodeLabel;
    [SerializeField] private TMP_Text lobbyCodeTMP;

    [SerializeField] private TMP_Text currentPlayersTMP;

    private string logHistory = "";
    private string _logText = "";
    private int subStringIndex = 0;
    public string logText
    {
        get { return _logText; }
        set
        {
            _logText = value;
            outputTMP.text = _logText.Substring(subStringIndex);
            if (outputTMP.textInfo.meshInfo[0].vertices.Length > 64000)
            {
                subStringIndex += (int)(outputTMP.text.Length * 0.5f);
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        publicServerBtn.onClick.AddListener(() => {
            logText += "Server button was clicked\n";
            ServerStartProcess(false);
            logText += "NetworkManager.Singleton.StartServer happened\n";
            publicServerBtn.enabled = false;
            publicServerBtn.gameObject.SetActive(false);
            privateServerBtn.enabled = false;
            privateServerBtn.gameObject.SetActive(false);
            shutdownBtn.gameObject.SetActive(true);
            shutdownBtn.enabled = true;
        });

        privateServerBtn.onClick.AddListener(() => {
            logText += "Private server button was clicked\n";
            ServerStartProcess(true);
            logText += "NetworkManager.Singleton.ServerStartProcess happened\n";
            publicServerBtn.enabled = false;
            publicServerBtn.gameObject.SetActive(false);
            privateServerBtn.enabled = false;
            privateServerBtn.gameObject.SetActive(false);
            shutdownBtn.gameObject.SetActive(true);
            shutdownBtn.enabled = true;
        });

        shutdownBtn.onClick.AddListener(() => {
            logText += "Shutdown button was clicked\n";

            ServerSideManager.I.StopLobbyHeartBeat();
            NetworkManager.Singleton.Shutdown();

            logText += "NetworkManager.Singleton.Shutdown happened\n";

            publicServerBtn.gameObject.SetActive(true);
            publicServerBtn.enabled = true;
            privateServerBtn.gameObject.SetActive(true);
            privateServerBtn.enabled = true;
            shutdownBtn.enabled = false;
            shutdownBtn.gameObject.SetActive(false);
        });

        managePromptsBtn.onClick.AddListener(() => {
            ServerManagePromptsModal.I.Display();
        });

        ServerManagePromptsModal.I.Hide();
    }

    public void WriteLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")}: {text}" : text;
        logText += $"{lineContent}\n";
    }
    
    public void WriteBadLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")} ERROR: {text}" : text;
        logText += $"<color=#FF0000>{lineContent}\n</color>";
    }

    public void EmptyOutput()
    {
        logHistory += logText;
        logText = "";
    }

    public void UpdateDisplayedLobbyCode(string lobbyCode)
    {
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            WriteLineToOutput($"There is a new private lobby code: {lobbyCode}");
        }
        lobbyCodeLabel.gameObject.SetActive(!string.IsNullOrEmpty(lobbyCode));
        lobbyCodeTMP.text = lobbyCode;
    }

    public void UpdatePlayerCounter(PlayerCountEventArgs e) 
    {
        WriteLineToOutput($"Changing player counter from {e.originalCount} to {e.newTotalCount}");
        if (e == null)
            return;

        currentPlayersTMP.text = e.newTotalCount.ToString();
    }

    private async void ServerStartProcess(bool privateLobby = false)
    {
        ServerSideManagerUI.I.WriteLineToOutput("In ServerStartProcess");
        await ServerSideManager.I.StartServer(privateLobby);
        ServerSideManagerUI.I.WriteLineToOutput("Server should be started");
    }

    public string GetFullLogText() {
        return logHistory + logText;
    }
}
