using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionControlUI : MonoBehaviour
{
    public static ConnectionControlUI I => instance;
    static ConnectionControlUI instance;


    [Header("Text outputs")]
    [SerializeField] private TMP_Text serverStatusTMP;

    [Header("Buttons")]
    [SerializeField] Button retryConnectingBtn;
    [SerializeField] Button switchToPrivateBtn;


    [Header("Inputs")]
    [SerializeField] TMP_InputField codeInputField;


    [Header("Subpanels")]
    [SerializeField] GameObject serverStatusGroup;
    [SerializeField] GameObject retryPublicGroup;
    [SerializeField] GameObject privateServerGroup;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        serverStatusTMP.text = "";
        codeInputField.text = string.Empty;

        retryConnectingBtn.onClick.AddListener(() => {
            _ = ClientSideManager.I.JoinPublicLobbyAndRelay();
        });

        switchToPrivateBtn.onClick.AddListener(() => {
            _ = ClientSideManager.I.JoinPrivateLobbyAndRelay(codeInputField.text.ToUpper());
        });

        ShowCurrentConnectionState();
    }

    public void ShowStartedConnecting()
    {
        serverStatusTMP.text = "Connecting";
        retryPublicGroup.SetActive(false);
        privateServerGroup.SetActive(false);
    }
    public void ShowStoppedConnecting(bool success, bool privateLobby)
    {
        if (success)
        {
            ShowSuccessfullyConnected(privateLobby);
        }
        else 
        {
            serverStatusTMP.text = "Offline";
            retryPublicGroup.SetActive(true);
            privateServerGroup.SetActive(true);
        }
    }
    public void ShowCurrentConnectionState()
    {
        if (ClientSideManager.I.IsConnectedToLobby(out var privateLobby))
        {
            ShowSuccessfullyConnected(privateLobby);
        }
        else 
        {
            ConnectionControlUI.I.ShowOffline();
        }
    }

    private void ShowSuccessfullyConnected(bool privateLobby)
    {
        serverStatusTMP.text = (privateLobby ? "Private" : "Public");
        retryPublicGroup.SetActive(privateLobby);
        privateServerGroup.SetActive(!privateLobby);
    }

    private void ShowOffline()
    {
        serverStatusTMP.text = ("Offline");
        retryPublicGroup.SetActive(true);
        privateServerGroup.SetActive(true);
    }
}
