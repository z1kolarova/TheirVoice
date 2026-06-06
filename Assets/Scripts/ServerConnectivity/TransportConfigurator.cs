using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class TransportConfigurator : MonoBehaviour
{
    private void Awake()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

#if UNITY_WEBGL
        transport.UseWebSockets = true;
#else
        transport.UseWebSockets = false;
#endif
    }
}
