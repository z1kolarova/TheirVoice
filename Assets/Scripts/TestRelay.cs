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

    private void Awake()
    {
        instance = this;
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
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(99); // 1 server + 99 clients = 100 => the max capacity

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManagerUI.I.WriteLineToOutput("Created relay allocation: " + joinCode);

            RelayServerData rsd = new RelayServerData(allocation, "dtls");
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

    public async void JoinRelayNewWay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartClient();

            NetworkManagerUI.I.WriteLineToOutput("Joined relay.");
        }
        catch (RelayServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    private async void CreateRelayOldWay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(99);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManagerUI.I.WriteLineToOutput("Created relay allocation: " + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }

    private async void JoinRelayOldWay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation. RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();

            NetworkManagerUI.I.WriteLineToOutput("Joined relay.");
        }
        catch (RelayServiceException e)
        {
            NetworkManagerUI.I.WriteLineToOutput(e.ToString());
        }
    }
}
