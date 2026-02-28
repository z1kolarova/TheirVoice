using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager I => instance;
    static RelayManager instance;

    private Allocation currentAllocation;
    private Task<string> creatingTask;

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
    }

    public async Task<string> CreateRelayNewWay()
    {
        if (creatingTask.IsWaitingForCompletion())
        {
            ServerSideManagerUI.I.WriteLineToOutput("Already waiting for relay creation");
            return await creatingTask;
        }

        creatingTask = CreateRelayInternal();
        return await creatingTask;
    }

    private async Task<string> CreateRelayInternal()
    {
        try
        {
            if (currentAllocation != null)
            {
                try
                {
                    var oldJoinCode = await RelayService.Instance.GetJoinCodeAsync(currentAllocation.AllocationId);
                    return oldJoinCode;
                }
                catch (System.Exception)
                {
                    ServerSideManagerUI.I.WriteBadLineToOutput("Tried to get code from old allocation, didn't work");
                }
            }

            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.ShutdownInProgress)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // 1 server + 99 clients = 100 => the max capacity
            // 1 server + 49 clients = 50 => the used value to fit within the free tier
            currentAllocation = await RelayService.Instance.CreateAllocationAsync(49);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(currentAllocation.AllocationId);

            ServerSideManagerUI.I.WriteLineToOutput("Created relay allocation: " + joinCode);

            RelayServerData rsd = AllocationUtils.ToRelayServerData(currentAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);

            NetworkManager.Singleton.StartServer();

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            return null;
        }
    }

    public async Task<bool> JoinRelayNewWay(string joinCode)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData rsd = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);

            Debug.Log("After joining relay, about to start client.");

            return NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e.Reason.ToString());
            Debug.Log(e.ToString());
            return false;
        }
    }
}
