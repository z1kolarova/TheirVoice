using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;

public class TestRelay : MonoBehaviour
{
    public static TestRelay I => instance;
    static TestRelay instance;

    private Allocation currentAllocation;

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        //await UnityServices.InitializeAsync();

        //AuthenticationService.Instance.SignedIn += () =>
        //{
        //    Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        //    NetworkManagerUI.I.WriteLineToOutput("Signed in " + AuthenticationService.Instance.PlayerId);
        //};

        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task<string> CreateRelayNewWay()
    {
        try
        {
            if (currentAllocation != null) {
                try
                {
                    var oldJoinCode = await RelayService.Instance.GetJoinCodeAsync(currentAllocation.AllocationId);
                    return oldJoinCode;
                }
                catch (System.Exception)
                {
                    NetworkManagerUI.I.WriteLineToOutput("Tried to get code from old allocation, didn't work");
                }
            }

            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.ShutdownInProgress)
            {
                NetworkManager.Singleton.Shutdown();
            }

            currentAllocation = await RelayService.Instance.CreateAllocationAsync(99); // 1 server + 99 clients = 100 => the max capacity

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(currentAllocation.AllocationId);

            NetworkManagerUI.I.WriteLineToOutput("Created relay allocation: " + joinCode);

            RelayServerData rsd = new RelayServerData(currentAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);

            NetworkManager.Singleton.StartServer();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            return null;
        }
    }

    public async Task<bool> JoinRelayNewWay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);

            //NetworkManagerUI.I.WriteLineToOutput("After joining relay, about to start client.");
            Debug.Log("After joining relay, about to start client.");

            return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            //NetworkManagerUI.I.WriteLineToOutput(e.Reason.ToString());
            Debug.Log(e.Reason.ToString());
            //NetworkManagerUI.I.WriteLineToOutput(e.ToString());
            Debug.Log(e.ToString());
            return false;
        }
    }
}
