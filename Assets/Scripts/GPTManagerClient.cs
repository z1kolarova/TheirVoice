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

    public override void OnNetworkSpawn()
    {
        //chatRequestToProcess.OnValueChanged += ChatRequestProcessing;
        //responseToProcess.OnValueChanged += ResponseChatMessageProcessing;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner || IsServer)
        {
            //NetworkManagerUI.I.WriteLineToOutput($"clientId {OwnerClientId}: connected with initial string: " + serialisedChatRequest.Value);
        }
    }

    private void ChatRequestProcessing(bool previousValue, bool newValue)
    {
        if (IsOwner || IsServer)
        {
            NetworkManagerUI.I.WriteLineToOutput($"clientId {OwnerClientId}: chatRequestToProcess changed from {previousValue} to {newValue}.");
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
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    chatRequestToProcess.Value = !chatRequestToProcess.Value;
        //}

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (IsClient)
            {
                ConvoUtilsGPT.InitNewConvoWithPrompt("You're a vegan actvist. Convince the person you're talking to to go vegan.");
                var request = ConvoUtilsGPT.GetSerialisedChatRequest("Hello.");
              
            
                NetworkManagerUI.I.WriteLineToOutput($"My request is: \"{request.Value}\"");
                TryGetGPTResponseServerRpc(OwnerClientId, request.Value.ToString());
                TryGetGPTResponseServerRpc(OwnerClientId, request.ToString());

            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (IsClient)
            {
                var request = ConvoUtilsGPT.GetSerialisedChatRequest("I'm listening. Continue.");
                NetworkManagerUI.I.WriteLineToOutput($"My request is: \"{request.Value}\"");
                TryGetGPTResponseServerRpc(OwnerClientId, request.Value.ToString());
            }
        }

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    NetworkManagerUI.I.WriteLineToOutput($"I recognise L press.");
        //    TestLobby.I.CheckForLobbies();
        //    NetworkManagerUI.I.WriteLineToOutput($"Check for lobbies should have happened.");
        //}

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    TestLobby.I.QuickJoinLobby();
        //    NetworkManagerUI.I.WriteLineToOutput($"Quickjoin should have happened.");
        //}
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
            NetworkManagerUI.I.WriteLineToOutput($"I'm a server asking API for response to {chatRequestFromClient}.");
            var res = await ConvoUtilsGPT.GetResponseAsServer(chatRequestFromClient);
            NetworkManagerUI.I.WriteLineToOutput($"I brought \"{res}\" as response.");
            ReceiveResponseClientRpc(res, SingleTarget(OwnerClientId));
        }
    }

    [ClientRpc]
    private void ReceiveResponseClientRpc(string chatMessageResponse, ClientRpcParams clientRpcParams = default)
    {
        NetworkManagerUI.I.WriteLineToOutput("I am in the ClientRpc.");
        NetworkManagerUI.I.WriteLineToOutput("param is: " + chatMessageResponse.ToString());
        ConvoUtilsGPT.ProcessResponseMessage(chatMessageResponse);
    }
}
