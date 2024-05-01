using JetBrains.Annotations;
using OpenAI_API.Chat;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GPTManagerClient : NetworkBehaviour
{
    //private NetworkVariable<FixedString4096Bytes> serialisedChatRequest = new NetworkVariable<FixedString4096Bytes>("", readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<FixedString4096Bytes> responseAsSerialisedChatMessage = new NetworkVariable<FixedString4096Bytes>("", readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<bool> chatRequestToProcess = new NetworkVariable<bool>(false, readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<bool> responseToProcess = new NetworkVariable<bool>(false, readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);

    private bool sayHello = true;

    public override void OnNetworkSpawn()
    {
        //NetworkManagerUI.I.WriteLineToOutput($"NetworkSpawn");
        Debug.Log($"NetworkSpawn");
        //chatRequestToProcess.OnValueChanged += ChatRequestProcessing;
        //responseToProcess.OnValueChanged += ResponseChatMessageProcessing;
    }

    // Start is called before the first frame update
    void Start()
    {
        //ConvoUtilsGPT.OnNewChatRequestToProcess += (object o, FixedString4096Bytes request) => {
        //    Debug.Log($"I'll try to get response to {request.Value}");
        //    TryGetGPTResponseServerRpc(OwnerClientId, request.Value.ToString());
        //    Debug.Log($"After the try...");
        //};
    }

    private void ChatRequestProcessing(bool previousValue, bool newValue)
    {
        if (IsOwner || IsServer)
        {
            //NetworkManagerUI.I.WriteLineToOutput($"clientId {OwnerClientId}: chatRequestToProcess changed from {previousValue} to {newValue}.");
        }

        if (newValue && IsOwner)
        {
            //NetworkManagerUI.I.WriteLineToOutput($"I am server, I know I'm being waited for and I'll do things.");
            //NetworkManagerUI.I.WriteLineToOutput($"Text I'll respond to is \"{testingString.Value}\"");
            //TryGetGPTResponseServerRpc(OwnerClientId, serialisedChatRequest.Value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ConvoUtilsGPT.HasNewChatRequestToProcess())
        {
            ConvoUtilsGPT.UpdateChatRequestBeganProcessing();
            Debug.Log($"HasNewChatRequestToProcess");
            var request = ConvoUtilsGPT.GetChatRequestToProcess();
            Debug.Log($"{request.Value}");

            TryGetGPTResponseServerRpc(OwnerClientId, request.Value.ToString());
        }
        if (sayHello)
        {
            //NetworkManagerUI.I.WriteLineToOutput($"Hello");
            Debug.Log($"Hello");
            sayHello = false;
        }
    }

    private ClientRpcParams SingleTarget(ulong clientId)
        => new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };


    [ServerRpc]
    private void TryGetGPTResponseServerRpc(ulong clientId, string chatRequestFromClient)
    {
        HandleGettingResponse(clientId, chatRequestFromClient);
    }

    private async void HandleGettingResponse(ulong clientId,string chatRequestFromClient)
    {
        if (IsServer)
        {
            //NetworkManagerUI.I.WriteLineToOutput($"I'm a server asking API for response to {chatRequestFromClient}.");
            var res = await ConvoUtilsGPT.GetResponseAsServer(chatRequestFromClient);
            //NetworkManagerUI.I.WriteLineToOutput($"I brought \"{res}\" as response.");
            ReceiveResponseClientRpc(res, SingleTarget(clientId));
        }
    }

    [ClientRpc]
    private void ReceiveResponseClientRpc(string chatMessageResponse, ClientRpcParams clientRpcParams = default)
    {
        //NetworkManagerUI.I.WriteLineToOutput("I am in the ClientRpc.");
        Debug.Log("I am in the ClientRpc.");
        //NetworkManagerUI.I.WriteLineToOutput("param is: " + chatMessageResponse.ToString());
        Debug.Log("param is: " + chatMessageResponse.ToString());
        ConvoUtilsGPT.ProcessResponseMessage(chatMessageResponse);
    }
}
