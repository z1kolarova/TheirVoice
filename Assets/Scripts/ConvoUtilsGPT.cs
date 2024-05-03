using Assets.Classes;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public static class ConvoUtilsGPT
{
    public static string PromptsDir = "./Assets/Prompts/";
    public static string PromptBankFileName = "_PromptBank";
    private static int USER_MSG_CHAR_LIMIT = 500;
    private static string CONVO_END_STRING = "#END_OF_CONVO#";
    private static string CONVO_END_INSTRUCTION = $"\r\nYou can choose to end the conversation whenever you decide (to end the conversation, append \"{CONVO_END_STRING}\" to the last message).";

    private static Model _model = Model.ChatGPTTurbo;   //originally was ChatGPTTurbo
    private static double _temperature = 0.5;           //originally was 0.1
    private static int _maxTokens = 256;                //originally was 50
    private static double _frequencyPenalty = 0.4;      //originally was 0
    private static double _presencePenalty = 0.4;       //originally was 0

    private static FixedString4096Bytes chatRequestToProcess = "";
    public static FixedString4096Bytes GetChatRequestToProcess() => chatRequestToProcess;

    private static bool currentlyWaitingForServerResponse = false;
    public static bool IsWaitingForResponse() => currentlyWaitingForServerResponse;

    private static bool hasNewChatRequestToProcess = false;
    public static bool HasNewChatRequestToProcess() => hasNewChatRequestToProcess;
    public static void UpdateChatRequestBeganProcessing() { hasNewChatRequestToProcess = false; }

    //public static event EventHandler<FixedString4096Bytes> OnNewChatRequestToProcess;

    private static OpenAIAPI api;
    public static OpenAIAPI API
    {
        get
        {
            if (api == null)
            {
                api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY_THEIR_VOICE", EnvironmentVariableTarget.User));
            }
            return api;
        }
    }

    private static List<ChatMessage> messages;
    public static List<ChatMessage> Messages
    {
        get
        {
            if (messages == null)
            {
                messages = new List<ChatMessage>();
            }
            return messages;
        }
    }

    public static Prompt CreatePrompt(string promptText, EndConvoAbility endConvoAbility, int chanceIfSometimes)
    {
        bool willBeAbleToEndConvo = false;
        switch (endConvoAbility)
        {
            case EndConvoAbility.Never:
                break;
            case EndConvoAbility.Sometimes:
                if (RngUtils.RollWithinLimitCheck(chanceIfSometimes))
                {
                    willBeAbleToEndConvo = true;
                    promptText += CONVO_END_INSTRUCTION;
                }
                break;
            case EndConvoAbility.Always:
                willBeAbleToEndConvo = true;
                promptText += CONVO_END_INSTRUCTION;
                break;
        }

        return new Prompt { 
            GeneralConvoEndingAbility = endConvoAbility,
            CanEndConvoThisTime = willBeAbleToEndConvo,
            Text = promptText
        };
    }
    //public static Prompt AddAbilityToEndConvo(this Prompt originalPrompt)
    //{ 
    //    originalPrompt.Text += CONVO_END_INSTRUCTION;
    //    return originalPrompt;
    //}

    public static void InitNewConvoWithPrompt(string prompt)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage(ChatMessageRole.System, prompt));
    }

    public static async Task<string> GetResponseTo(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return "";
        }

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = msgText.Trim();

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        Messages.Add(userMessage);


        var chatResult = await API.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = _model,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            Messages = messages,
            FrequencyPenalty = _frequencyPenalty,
            PresencePenalty = _presencePenalty
        });

        if (chatResult == null || chatResult.Choices.Count == 0)
        {
            Debug.Log("ChatGPT didn't give back chatResult");
        }
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;

        messages.Add(responseMessage);

        Debug.Log(responseMessage.Content);
        return responseMessage.Content;
    }

    public static void GetServerResponseTo(string msgText)
    {
        Debug.Log(msgText);
        var request = ConvoUtilsGPT.GetSerialisedChatRequest(msgText);
        if (request.HasValue)
        {
            Debug.Log("request has value");
            chatRequestToProcess = request.Value;
            //OnNewChatRequestToProcess?.Invoke(null, request.Value);
            hasNewChatRequestToProcess = true;
            currentlyWaitingForServerResponse = true;

            Debug.Log($"things should be true {hasNewChatRequestToProcess} {currentlyWaitingForServerResponse}");
        }
    }

    public static ChatRequest GetChatRequest(string msgText)
    {
        Debug.Log($"checking msgText {msgText}");
        if (TryAddUserResponse(msgText))
        {
            Debug.Log($"msgText is ok");
            return ProduceChatRequest();
        }
        Debug.Log($"msgText is NOT ok");
        return null;
    }

    public static FixedString4096Bytes? GetSerialisedChatRequest(string msgText)
    {
        Debug.Log($"checking msgText {msgText}");
        if (TryAddUserResponse(msgText))
        {
            Debug.Log($"msgText is ok");
            return ProduceSerialisedChatRequest();
        }
        Debug.Log($"msgText is NOT ok");
        return null;
    }

    public static bool TryAddUserResponse(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return false;
        }

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = msgText.Trim();

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        messages.Add(userMessage);

        return true;
    }

    public static ChatRequest ProduceChatRequest() =>
        new ChatRequest()
        {
            Model = _model,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            Messages = messages,
            FrequencyPenalty = _frequencyPenalty,
            PresencePenalty = _presencePenalty
        };

    public static string ProduceSerialisedChatRequest()
    {
        var chatRequest = new ChatRequest()
        {
            Model = _model,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            Messages = messages,
            FrequencyPenalty = _frequencyPenalty,
            PresencePenalty = _presencePenalty
        };
        return JsonConvert.SerializeObject(chatRequest);
    }

    public static async Task<string> GetResponseAsServer(string serialisedChatRequest)
    {
        var chatRequest = JsonConvert.DeserializeObject<ChatRequest>(serialisedChatRequest.ToString());
        var chatResult = await API.Chat.CreateChatCompletionAsync(chatRequest);
        
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;

        var serialisedChatMessage = JsonConvert.SerializeObject(responseMessage);

        return serialisedChatMessage;
    }

    public static void ProcessResponseMessage(string serialisedResponseMessage)
    {
        //NetworkManagerUI.I.WriteLineToOutput("I am in ProcessChatResult.");
        Debug.Log("I am in ProcessChatResult.");
        //NetworkManagerUI.I.WriteLineToOutput(serialisedResponseMessage.ToString());
        Debug.Log(serialisedResponseMessage.ToString());
        ChatMessage responseMessage = JsonConvert.DeserializeObject<ChatMessage>(serialisedResponseMessage.ToString());

        //NetworkManagerUI.I.WriteLineToOutput("responseMessage.Content:");
        Debug.Log("responseMessage.Content:");
        //NetworkManagerUI.I.WriteLineToOutput(responseMessage.Content);
        Debug.Log(responseMessage.Content);
        messages.Add(responseMessage);

        ConversationUIChatGPT.I.SetNewDialogueToDisplay(responseMessage.Content);
        currentlyWaitingForServerResponse = false;
    }

    public static async Task<string> FakeGettingResponseTo(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return "";
        }

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = msgText.Trim();

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        messages.Add(userMessage);

        var responseText = RngUtils.CoinFlip()
            ? "This is a very profound (but fake) answer to what you just said." 
            : $"I'm gonna go now.{CONVO_END_STRING}";
        
        var responseMessage = new ChatMessage(ChatMessageRole.Assistant, responseText);
        messages.Add(responseMessage);

        Debug.Log("A fake response was obtained");
        return responseMessage.Content;
    }

    public static bool WillEndConvo(this string msgText, out string msgToUse)
    {
        var result = msgText.EndsWith(CONVO_END_STRING);
        msgToUse = msgText.Replace(CONVO_END_STRING, "");
        return result;
    }

    public static List<PromptLabel> GetPromptBank()
    {
        var path = Path.Combine(PromptsDir, $"{PromptBankFileName}.json");
        List<PromptLabel> result = new List<PromptLabel>();

        using (StreamReader sr = new StreamReader(path))
        using (JsonReader jr = new JsonTextReader(sr))
        {
            result = Utilities.Serializer.Deserialize<List<PromptLabel>>(jr);
        }

        return result;
    }

    public static string GetPromptTextByLabel(PromptLabel label)
    {
        var path = Path.Combine(PromptsDir, $"{label.Name}.txt");

        if (!File.Exists(path))
        {
            return null;
        }

        string text = File.ReadAllText(path);
        return text;
    }

    public static void SerializePromptBank(List<PromptLabel> promptLabels)
    {
        var filePath = Path.Combine(PromptsDir, $"{PromptBankFileName}.json");
        
        using (StreamWriter sw = new StreamWriter(filePath))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            Utilities.Serializer.Serialize(writer, promptLabels);
        }
    }
    public static void SerializePrompt<T>(T prompt, string fileName)
    {
        var filePath = Path.Combine(PromptsDir, $"{fileName}.json");
        using (StreamWriter sw = new StreamWriter(filePath))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            Utilities.Serializer.Serialize(writer, prompt);
        }
    }
}
