using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerPrefabRpcs : NetworkBehaviour
{
    //private NetworkVariable<FixedString4096Bytes> serialisedChatRequest = new NetworkVariable<FixedString4096Bytes>("", readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<FixedString4096Bytes> responseAsSerialisedChatMessage = new NetworkVariable<FixedString4096Bytes>("", readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<bool> chatRequestToProcess = new NetworkVariable<bool>(false, readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<bool> responseToProcess = new NetworkVariable<bool>(false, readPerm: NetworkVariableReadPermission.Owner, writePerm: NetworkVariableWritePermission.Owner);

    private List<string> chunksClient;
    private List<string> chunksServer;
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
        DontDestroyOnLoad(this.gameObject);
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
        if (ClientDataUtils.AvailableLangsRequester.NeedsData())
        {
            ClientDataUtils.AvailableLangsRequester.UpdateBeganProcessing();
            Debug.Log("needs available languages was true");
            GetLanguagesServerRpc(OwnerClientId);
        }

        if (ClientDataUtils.SystemPromptsRequester.NeedsData())
        {
            ClientDataUtils.SystemPromptsRequester.UpdateBeganProcessing();
            Debug.Log("needs system prompts was true");
            GetSystemPromptsServerRpc(OwnerClientId);
        }

        if (ClientDataUtils.AvailablePromptsRequester.NeedsData())
        {
            ClientDataUtils.AvailablePromptsRequester.UpdateBeganProcessing();
            Debug.Log("needs available prompts was true");
            GetPromptsServerRpc(OwnerClientId, UserSettingsManager.I.ConversationLanguage.Id);
        }

        if (ClientDataUtils.PromptLocRequester.NeedsNewData())
        {
            var queue = ClientDataUtils.PromptLocRequester.DataKeyQueue();
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                var promptId = queue[i].Item1;
                var langId = queue[i].Item2;
                Debug.Log($"needs promptLoc for {promptId}");
                ClientDataUtils.PromptLocRequester.UpdateBeganProcessing(queue[i]);
                GetPromptLocServerRpc(OwnerClientId, promptId, langId);
            }
        }

        #region GPT response
        if (ConvoUtilsGPT.ChatGPTResponseRequester.NeedsData())
        {
            ConvoUtilsGPT.ChatGPTResponseRequester.UpdateBeganProcessing();
            Debug.Log($"HasNewChatRequestToProcess");
            var request = ConvoUtilsGPT.GetChatRequestToProcess();
            Debug.Log($"{request.Value}");

            TryGetGPTResponseServerRpc(OwnerClientId, request.Value.ToString());
        }
        #endregion GPT response

        #region Whisper
        if (AudioUtilsWhisper.HasNewRequestToProcess())
        {
            AudioUtilsWhisper.UpdateRequestBeganProcessing();
            Debug.Log($"HasNewWhisperRequestToProcess");
            StartChunkAccumulation();
        }
        #endregion Whisper

        #region debug Hello
        if (sayHello)
        {
            Debug.Log($"Hello");
            sayHello = false;
        }
        #endregion debug Hello
    }

    private ClientRpcParams SingleTarget(ulong clientId)
        => new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } };

    #region client-host data exchange
    #region LanguagesRpcs
    [ServerRpc]
    private void GetLanguagesServerRpc(ulong clientId)
    {
        HandleGettingLanguages(clientId);
    }

    private void HandleGettingLanguages(ulong clientId)
    {
        if (IsServer)
        {
            try
            {
                var res = DBServiceUtils.GetLanguagesWithAvailablePromptLocs().ToArray();
                var serialised = JsonConvert.SerializeObject(res);
                ReceiveLanguagesClientRpc(serialised, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }

    [ClientRpc]
    private void ReceiveLanguagesClientRpc(string serialisedLanguages, ClientRpcParams clientRpcParams = default)
    {
        ClientDataUtils.ProcessAvailableLanguages(serialisedLanguages);
    }
    #endregion LanguagesRpcs

    #region PromptsRpcs
    [ServerRpc]
    private void GetSystemPromptsServerRpc(ulong clientId)
    {
        HandleGettingSystemPrompts(clientId);
    }

    private void HandleGettingSystemPrompts(ulong clientId)
    {
        if (IsServer)
        {
            try
            {
                var res = DBServiceUtils.GetSystemPrompts().ToArray();
                var serialised = JsonConvert.SerializeObject(res);
                ReceiveSystemPromptsClientRpc(serialised, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }

    [ClientRpc]
    private void ReceiveSystemPromptsClientRpc(string serialisedPrompts, ClientRpcParams clientRpcParams = default)
    {
        ClientDataUtils.ProcessSystemPrompts(serialisedPrompts);
    }

    [ServerRpc]
    private void GetPromptsServerRpc(ulong clientId, int languageId)
    {
        HandleGettingPrompts(clientId, languageId);
    }

    private void HandleGettingPrompts(ulong clientId, int languageId)
    {
        if (IsServer)
        {
            try
            {
                var res = DBServiceUtils.GetReadyToUseNonSystemPromptsInLanguage(languageId).ToArray();
                var serialised = JsonConvert.SerializeObject(res);
                ReceivePromptsClientRpc(serialised, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }

    [ClientRpc]
    private void ReceivePromptsClientRpc(string serialisedPrompts, ClientRpcParams clientRpcParams = default)
    {
        ClientDataUtils.ProcessAvailablePrompts(serialisedPrompts);
    }
    #endregion PromptsRpcs

    #region PromptLocRpcs
    [ServerRpc]
    private void GetPromptLocServerRpc(ulong clientId, int promptId, int languageId)
    {
        HandleGettingPromptLocText(clientId, promptId, languageId);
    }

    private void HandleGettingPromptLocText(ulong clientId, int promptId, int languageId)
    {
        if (IsServer)
        {
            try
            {
                if(!(ServerDataUtils.ChunksReadyDic.TryGetValue((promptId, languageId), out var ready) && ready))
                {
                    var promptLocText = DBServiceUtils.GetPromptLocText(promptId, languageId);

                    var chunks = new List<string>();
                    Utils.ChunkData(promptLocText, ref chunks);

                    ServerDataUtils.ChunkStorageDic[(promptId, languageId)] = chunks;
                    ServerDataUtils.ChunksReadyDic[(promptId, languageId)] = true;
                }

                ServerDataUtils.ChunkStorageDic.TryGetValue((promptId, languageId), out var allChunks);
                var chunkCount = allChunks.Count;
                Debug.Log(chunkCount);
                for (int i = 0; i < chunkCount; i++)
                {
                    var chunk = allChunks[i];
                    ReceivePromptLocChunkClientRpc(promptId, languageId, chunk, i == chunkCount - 1, SingleTarget(clientId));
                }
                Debug.Log("after accumulation");
            }
            catch (Exception e)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }

    [ClientRpc]
    private void ReceivePromptLocChunkClientRpc(int promptId, int langId, string chunk, bool isLast = false, ClientRpcParams clientRpcParams = default)
    {
        ReceivePromptLocChunk(promptId, langId, chunk, isLast);
    }

    private void ReceivePromptLocChunk(int promptId, int langId, string chunk, bool isLast)
    {
        try
        {
            if (!ClientDataUtils.ChunkStorageDic.TryGetValue((promptId, langId), out var chunks))
            {
                chunks = new List<string>();
            }
            chunks.Add(chunk);
            ClientDataUtils.ChunkStorageDic[(promptId, langId)] = chunks;

            if (isLast)
            {
                ClientDataUtils.CachePromptLocText(promptId, langId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    #endregion PromptLocRpcs
    #endregion client-host data exchange

    #region ChatGPT
    [ServerRpc]
    private void TryGetGPTResponseServerRpc(ulong clientId, string chatRequestFromClient)
    {
        HandleGettingResponse(clientId, chatRequestFromClient);
    }

    private async void HandleGettingResponse(ulong clientId, string chatRequestFromClient)
    {
        if (IsServer)
        {
            try
            {
                ServerSideManagerUI.I.WriteLineToOutput($"I'm a server asking API for response to {chatRequestFromClient}.");
                var res = await ConvoUtilsGPT.GetResponseAsServer(chatRequestFromClient);
                ServerSideManagerUI.I.WriteLineToOutput($"I brought \"{res}\" as response.");
                ReceiveResponseClientRpc(res, SingleTarget(clientId));
            }
            catch (Exception e)
            {
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
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
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            }
        }
    }
    #endregion

    #region Whisper (unused?)
    [ServerRpc]
    private void TryGetWhisperTranscriptServerRpc(ulong clientId, string transcriptRequestFromClient)
    {
        ServerSideManagerUI.I.WriteLineToOutput($"Client {clientId} requests transcript.");
        HandleGettingTranscript(clientId, transcriptRequestFromClient);
        ServerSideManagerUI.I.WriteLineToOutput($"Server has delivered the requested transcript.");
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
                ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
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
