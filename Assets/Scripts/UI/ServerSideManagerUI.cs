using Assets.Classes;
using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ServerSideManagerUI : MonoBehaviour
{
    public static ServerSideManagerUI I => instance;
    static ServerSideManagerUI instance;

    [Header("API key selection")]
    [SerializeField] private TMP_Dropdown keySelectionDropdown;

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
            outputTMP.ForceMeshUpdate();
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
        keySelectionDropdown.onValueChanged.AddListener(delegate
        {
            CommunicateKeySelection();
        });

        publicServerBtn.onClick.AddListener(() => {
            ServerSideManagerUI.I.WriteCyanLineToOutput("Public server button was clicked");
            LockBeforeServerLaunch();
            ServerStartProcess(false);
            ServerSideManagerUI.I.WriteLineToOutput("NetworkManager.Singleton.StartServer happened");
            ShowAfterServerLaunch();
        });

        privateServerBtn.onClick.AddListener(() => {
            ServerSideManagerUI.I.WriteCyanLineToOutput("Private server button was clicked");
            LockBeforeServerLaunch();
            ServerStartProcess(true);
            ServerSideManagerUI.I.WriteLineToOutput("NetworkManager.Singleton.StartServer happened");
            ShowAfterServerLaunch();
        });

        shutdownBtn.onClick.AddListener(() => {
            ServerSideManagerUI.I.WriteCyanLineToOutput("Shutdown button was clicked");

            ServerSideManager.I.ShutDownHostLobby();
            NetworkManager.Singleton.Shutdown();

            ServerSideManagerUI.I.WriteLineToOutput("NetworkManager.Singleton.Shutdown happened");

            publicServerBtn.gameObject.SetActive(true);
            publicServerBtn.enabled = true;
            privateServerBtn.gameObject.SetActive(true);
            privateServerBtn.enabled = true;
            shutdownBtn.enabled = false;
            shutdownBtn.gameObject.SetActive(false);
            ServerSideManagerUI.I.SetKeySelectionInteractable(APIKeyManager.I.IsKeySelected);
        });

        managePromptsBtn.onClick.AddListener(() => {
            ServerSideManagerUI.I.WriteCyanLineToOutput("ManagePrompts button was clicked");
            ServerManagePromptsModal.I.Display();
        });

        ServerManagePromptsModal.I.Hide();
        ServerEditPromptModal.I.Hide();
        PopulateDropdownWithKeyOptions();
    }

    public void WriteLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")}: {text}" : text;
        logText += $"{lineContent}\n";
    }
    public void WriteCyanLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")} {text}" : text;
        logText += $"<color=#00FFFF>{lineContent}</color>\n";
    }

    public void WriteYellowLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")} {text}" : text;
        logText += $"<color=#FFFF00>{lineContent}</color>\n";
    }

    public void WriteBadLineToOutput(string text, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")} ERROR: {text}" : text;
        logText += $"<color=#FF0000>{lineContent}</color>\n";
    }

    public void WriteLineToOutputWithColor(string text, Color color, bool timestamp = true)
    {
        var lineContent = timestamp ? $"{DateTime.Now.ToString("HH:mm:ss")} {text}" : text;
        logText += $"<color=#{color.ToHexString()}>{lineContent}</color>\n";
        // Color.yellow is not #FFFF00 but #FFEB04 so I don't like using this for yellow
        // white    is #FFFFFF, as it should be, and looks good
        // magenta  is #FF00FF, as it should be, and looks good
        // cyan     is #00FFFF, as it should be, and looks good
        // red      is #FF0000, as it should be, but is too dark
        // green    is #00FF00, as it should be, but I like the chosen default shade #67FF01 better
        // blue     is #0000FF, as it should be, but is too dark
        // black    is #000000, as it should be, and surprisingly still readable
        // gray and clear are not worth using here
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
            I.WriteLineToOutputWithColor($"There is a new private lobby code: {lobbyCode}", Color.white);
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

    private void LockBeforeServerLaunch()
    {
        SetKeySelectionInteractable(false);
        if (!APIKeyManager.I.IsKeySelected)
        {
            CommunicateKeySelection();
        }

        publicServerBtn.enabled = false;
        publicServerBtn.gameObject.SetActive(false);
        privateServerBtn.enabled = false;
        privateServerBtn.gameObject.SetActive(false);
    }

    private void ShowAfterServerLaunch()
    {
        shutdownBtn.gameObject.SetActive(true);
        shutdownBtn.enabled = true;
    }

    private void PopulateDropdownWithKeyOptions()
    {
        keySelectionDropdown.options.Clear();

        var keyNameOptions = APIKeyManager.I.GetKeyNameOptions();

        foreach (var keyName in keyNameOptions)
        {
            keySelectionDropdown.options.Add(new TMP_Dropdown.OptionData(keyName));
        }

        if (APIKeyManager.I.IsKeySelected)
        {
            keySelectionDropdown.value = keyNameOptions.IndexOf(APIKeyManager.I.SelectedKeyName);
        }
        else
        {
            keySelectionDropdown.value = 0;
        }

        keySelectionDropdown.RefreshShownValue();

        if (keySelectionDropdown.options.Count == 0)
        {
            SetKeySelectionInteractable(false);
        }
    }

    private void CommunicateKeySelection()
    {
        APIKeyManager.I.SetSelectedKeyName(keySelectionDropdown.options[keySelectionDropdown.value].text);
    }

    private void SetKeySelectionInteractable(bool interactable)
    {
        keySelectionDropdown.interactable = interactable;
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
