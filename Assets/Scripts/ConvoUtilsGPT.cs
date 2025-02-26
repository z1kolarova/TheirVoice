using Assets.Classes;
using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using static Constants;

public static class ConvoUtilsGPT
{
    public static PromptLabel notInterestedPromptLabel = new PromptLabel() { 
        Name = NOT_INTERESTED_PROMPT_NAME,
        EndConvoAbility = EndConvoAbility.Always,
        Tags = ArgumentationTag.None
    };

    private static ChatMessage safetyNetMessage = new ChatMessage() { 
        Role = "assistant", //ChatMessageRole.Assistant, 
        Content = "I'm gonna go now." + CONVO_END_STRING
    };

    private static string _model = "gpt-4o-mini";       //originally was Model.ChatGPTTurbo when using different API
    private static float _temperature = 0.5f;           //originally was 0.1
    private static int _maxTokens = 1050;               //originally was 256
    private static float _frequencyPenalty = 0.4f;      //originally was 0
    private static float _presencePenalty = 0.4f;       //originally was 0

    private static FixedString4096Bytes chatRequestToProcess = "";
    public static FixedString4096Bytes GetChatRequestToProcess() => chatRequestToProcess;

    private static bool currentlyWaitingForServerResponse = false;
    public static bool IsWaitingForResponse() => currentlyWaitingForServerResponse;

    private static bool hasNewChatRequestToProcess = false;
    public static bool HasNewChatRequestToProcess() => hasNewChatRequestToProcess;
    public static void UpdateChatRequestBeganProcessing() { hasNewChatRequestToProcess = false; }

    //public static event EventHandler<FixedString4096Bytes> OnNewChatRequestToProcess;

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

    public static Prompt CreatePrompt(string promptText, EndConvoAbility endConvoAbility, int chanceIfSometimes = 50)
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

    public static Prompt CreateNotInterestedPrompt()
    {
        var text = GetPromptTextByLabel(notInterestedPromptLabel);
        return CreatePrompt(text, notInterestedPromptLabel.EndConvoAbility);
    }

    public static void InitNewConvoWithPrompt(string prompt)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage() { Role = "system", Content = prompt });
    }

    public static async Task<string> GetResponseTo(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return "";
        }

        ChatMessage userMessage = new ChatMessage() { 
            Role = "user", 
            Content = msgText.Trim() 
        };

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.Content));

        Messages.Add(userMessage);


        var chatResult = await API.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = _model,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            Messages = messages,
            FrequencyPenalty = _frequencyPenalty,
            PresencePenalty = _presencePenalty
        });

        if (chatResult.Choices.Count == 0)
        {
            Debug.Log("ChatGPT didn't give back chatResult");
        }

        ChatMessage responseMessage = new ChatMessage()
        {
            Role = chatResult.Choices[0].Message.Role,
            Content = chatResult.Choices[0].Message.Content
        };

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
            chatRequestToProcess = request.Value;
            hasNewChatRequestToProcess = true;
            currentlyWaitingForServerResponse = true;
        }
        else
        {
            ConversationUIChatGPT.I.SetNewDialogueToDisplay(safetyNetMessage.Content);
            currentlyWaitingForServerResponse = false;
        }
    }

    public static FixedString4096Bytes? GetSerialisedChatRequest(string msgText)
    {
        if (TryAddUserResponse(msgText))
        {
            Debug.Log("after tryAddUserResponse");
            var serialised = ProduceSerialisedChatRequest();
            if (serialised.Length < 4093)
            {
                return serialised;
            } 
        }
        return null;
    }

    public static bool TryAddUserResponse(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return false;
        }

        ChatMessage userMessage = new ChatMessage()
        {
            Role = "user",
            Content = msgText.Trim()
        };

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        messages.Add(userMessage);

        return true;
    }

    public static CreateChatCompletionRequest ProduceChatRequest() =>
        new CreateChatCompletionRequest()
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
        try
        {
            var chatRequest = ProduceChatRequest();
            return JsonConvert.SerializeObject(chatRequest);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<string> GetResponseAsServer(string serialisedChatRequest)
    {
        var chatRequest = JsonConvert.DeserializeObject<CreateChatCompletionRequest>(serialisedChatRequest.ToString());
        try
        {
            var chatResult = await API.CreateChatCompletion(chatRequest);
            ChatMessage responseMessage = new ChatMessage()
            {
                Role = chatResult.Choices[0].Message.Role,
                Content = chatResult.Choices[0].Message.Content
            };

            var serialisedChatMessage = JsonConvert.SerializeObject(responseMessage);

            return serialisedChatMessage;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return JsonConvert.SerializeObject(safetyNetMessage);
        }
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

    public static Task<string> FakeGettingResponseTo(string msgText)
    {
        if (string.IsNullOrWhiteSpace(msgText))
        {
            return Task.FromResult("");
        }

        ChatMessage userMessage = new ChatMessage()
        {
            Role = "user",
            Content = msgText.Trim()
        };

        if (userMessage.Content.Length > USER_MSG_CHAR_LIMIT)
        {
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.Role, userMessage.Content));

        messages.Add(userMessage);

        var responseText = RngUtils.CoinFlip()
            ? "This is a very profound (but fake) answer to what you just said." 
            : $"I'm gonna go now.{CONVO_END_STRING}";

        ChatMessage responseMessage = new ChatMessage()
        {
            Role = "assistant",
            Content = responseText
        };
        messages.Add(responseMessage);

        Debug.Log("A fake response was obtained");
        return Task.FromResult(responseMessage.Content);
    }

    public static bool WillEndConvo(this string msgText, out string msgToUse)
    {
        var result = msgText.EndsWith(CONVO_END_STRING);
        msgToUse = msgText.Replace(CONVO_END_STRING, "");
        return result;
    }

    public static List<PromptLabel> GetPromptBank()
    {
        var path = Path.Combine(PromptsDir, PromptBankFileName);
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
        var filePath = Path.Combine(PromptsDir, PromptBankFileName);
        
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
