using JetBrains.Annotations;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
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

    private List<string> chunksClient;
    private List<string> chunksServer;
    private int totalChunks = 0;
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
        chunksClient = new List<string>();
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
        if (AudioUtilsWhisper.HasNewRequestToProcess())
        {
            AudioUtilsWhisper.UpdateRequestBeganProcessing();
            Debug.Log($"HasNewWhisperRequestToProcess");
            StartChunkAccumulation();


            //var request = AudioUtilsWhisper.GetTranscriptChunk();
            //Debug.Log($"{request.Value}");

            //TryGetWhisperTranscriptServerRpc(OwnerClientId, request.Value.ToString());
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

    #region ChatGPT
    [ServerRpc]
    private void TryGetGPTResponseServerRpc(ulong clientId, string chatRequestFromClient)
    {
        HandleGettingResponse(clientId, chatRequestFromClient);
    }

    private async void HandleGettingResponse(ulong clientId,string chatRequestFromClient)
    {
        if (IsServer)
        {
            try
            {
                //NetworkManagerUI.I.WriteLineToOutput($"I'm a server asking API for response to {chatRequestFromClient}.");
                var res = await ConvoUtilsGPT.GetResponseAsServer(chatRequestFromClient);
                //NetworkManagerUI.I.WriteLineToOutput($"I brought \"{res}\" as response.");
                ReceiveResponseClientRpc(res, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                NetworkManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
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
    #endregion

    #region Chunky
    private void StartChunkAccumulation()
    {
        var allChunks = AudioUtilsWhisper.GetAllChunks();
        var chunkCount = allChunks.Count;
        Debug.Log(chunkCount);
        for (int i = 0; i < chunkCount; i++)
        {
            var chunk = allChunks[i];
            AccumulateChunksServerRpc(OwnerClientId, chunk, i == chunkCount - 1);
        }
        Debug.Log("after accumulation");
    }
    [ServerRpc]
    private void AccumulateChunksServerRpc(ulong clientId, string chunk, bool isLast = false)
    {
        ReceiveRawChunk(clientId, chunk, isLast);
    }

    private async void ReceiveRawChunk(ulong clientId, string chunk, bool isLast)
    {
        if (IsServer)
        {
            try
            {
                AudioUtilsWhisper.GetAllChunks().Add(chunk);
                if (isLast)
                {
                    var request = AudioUtilsWhisper.DechunkDataToSerialisedRequest();
                    AudioUtilsWhisper.GetAllChunks().Clear();
                    var res = await AudioUtilsWhisper.GetResponseAsServer(request);
                    ReceiveWhisperResponseClientRpc(res, SingleTarget(clientId));
                }
            }
            catch (Exception e)
            {
                NetworkManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }
    #endregion

    #region Whisper
    [ServerRpc]
    private void TryGetWhisperTranscriptServerRpc(ulong clientId, string transcriptRequestFromClient)
    {
        HandleGettingTranscript(clientId, transcriptRequestFromClient);
    }

    private async void HandleGettingTranscript(ulong clientId, string transcriptRequestFromClient)
    {
        if (IsServer)
        {
            try
            {
                var res = await AudioUtilsWhisper.GetResponseAsServer(transcriptRequestFromClient);
                ReceiveWhisperResponseClientRpc(res, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                NetworkManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }

    [ClientRpc]
    private void ReceiveWhisperResponseClientRpc(string transcriptResponse, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("I am in the ClientRpc.");
        Debug.Log("param is: " + transcriptResponse.ToString());
        AudioUtilsWhisper.ProcessResult(transcriptResponse);
    }
    #endregion
}
