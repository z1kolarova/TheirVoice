using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;
using Unity.Collections;
using UnityEngine;

public static class AudioUtilsWhisper
{
    private static string _fileName = "output.wav";
    private static string _model = "whisper-1";
    private static int MaxChunkSize = 4000;

    private static List<string> chunksToProcess = new List<string>();
    public static List<string> GetAllChunks() => chunksToProcess;
    private static FixedString4096Bytes chunkToProcess = "";
    public static FixedString4096Bytes GetTranscriptChunk() => chunkToProcess;
    private static bool hasNewChunkToProcess = false;
    public static bool HasNewChunkToProcess() => hasNewChunkToProcess;
    
    private static bool needsNewChunkToProcess = false;
    public static bool NeedsNewChunkToProcess() => needsNewChunkToProcess;

    private static FixedString4096Bytes transcriptionRequestToProcess = "";
    public static FixedString4096Bytes GetTranscriptRequest() => transcriptionRequestToProcess;

    private static bool currentlyWaitingForServerResponse = false;
    public static bool IsWaitingForResponse() => currentlyWaitingForServerResponse;

    private static bool hasNewRequestToProcess = false;
    public static bool HasNewRequestToProcess() => hasNewRequestToProcess;
    public static bool NeedsMoreChunks() => hasNewRequestToProcess;
    public static void UpdateRequestBeganProcessing() { hasNewRequestToProcess = false; }

    private static OpenAIApi api;
    public static OpenAIApi API
    {
        get
        {
            if (api == null)
            {
                ServerSideManagerUI.I.WriteLineToOutput("using API key " + APIKeyManager.I.SelectedKeyName);
                api = new OpenAIApi(Environment.GetEnvironmentVariable(APIKeyManager.I.SelectedKeyName, EnvironmentVariableTarget.User));
            }
            return api;
        }
    }

    public static void ChunkData(byte[] data)
    {
        chunksToProcess.Clear();
        for (int i = 0; i < data.Length; i+= MaxChunkSize)
        {
            int length = Math.Min(MaxChunkSize, data.Length - i);
            byte[] chunk = new byte[length];
            Array.Copy(data, i, chunk, 0, length);
            chunksToProcess.Add(Convert.ToBase64String(chunk));
        }
        hasNewRequestToProcess = true;
        currentlyWaitingForServerResponse = true;
    }

    public static string DechunkDataToSerialisedRequest()
    {
        List<byte> data = new List<byte>();
        for (int i = 0; i < chunksToProcess.Count; i++)
        {
            byte[] chunk = Convert.FromBase64String(chunksToProcess[i]);
            data.AddRange(chunk);
        }
        var request = GetSerialisedRequest(data.ToArray());
        return request;
    }

    public static void GetTranscriptionThroughServer(byte[] data, string language = "en")
    {
        Debug.Log("Beginning of GetTranscriptionThroughServer");
        ChunkData(data);
    }

    private static string GetSerialisedRequest(byte[] data, string language = "en")
    {
        try
        {
            Debug.Log(data.Length);
            var transcriptRequest = ProduceRequest(data, language);
            Debug.Log(transcriptRequest);
            return JsonConvert.SerializeObject(transcriptRequest);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public static CreateAudioTranscriptionsRequest ProduceRequest(byte[] data, string language = "en")
        => new CreateAudioTranscriptionsRequest()
        {
            FileData = new FileData() { Data = data, Name = _fileName },
            Model = _model,
            Language = language,
        };

    public static async Task<string> GetResponseAsServer(string serialisedRequest)
    {
        var request = JsonConvert.DeserializeObject<CreateAudioTranscriptionsRequest>(serialisedRequest.ToString());
        try
        {
            var res = await API.CreateAudioTranscription(request);
            var serialisedResponse = JsonConvert.SerializeObject(res);

            return serialisedResponse;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return "Transcription failed";
        }
    }

    public static void ProcessResult(string serialisedTranscriptResponse)
    {
        Debug.Log("I am in ProcessResult.");

        CreateAudioResponse transcriptResponse = JsonConvert.DeserializeObject<CreateAudioResponse>(serialisedTranscriptResponse.ToString());
        Debug.Log(transcriptResponse.Text);

        ConversationUIChatGPT.I.SetUserInputText(transcriptResponse.Text);
        currentlyWaitingForServerResponse = false;
    }
}
