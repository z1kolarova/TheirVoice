using System;
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

    private void Awake()
    {
        instance = this;

        serverBtn.onClick.AddListener(() => { 
            outputTMP.text += "Server button was clicked\n";
            NetworkManager.Singleton.StartServer();
            outputTMP.text += "NetworkManager.Singleton.StartServer happened\n";
            serverBtn.enabled = false;
            serverBtn.gameObject.SetActive(false);
            clientBtn.enabled = false;
            clientBtn.gameObject.SetActive(false);
            shutdownBtn.enabled = true;
        });

        clientBtn.onClick.AddListener(() => {
            outputTMP.text += "Client button was clicked\n";
            NetworkManager.Singleton.StartClient();
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
        });

        outputTMP.text = "";
        outputTMP.text += "I am awake\n";
    }

    public void Start()
    {
    }

    public void WriteLineToOutput(string text)
    {
        outputTMP.text += text + "\n";
    }


    private void ClientStart()
    {
        outputTMP.text += "Server button was clicked\n";
        NetworkManager.Singleton.StartServer();
        outputTMP.text += "NetworkManager.Singleton.StartServer happened\n";
    }
}
