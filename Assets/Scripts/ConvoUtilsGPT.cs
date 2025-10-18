using Assets.Classes;
using Assets.Enums;
using Newtonsoft.Json;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using static Constants;

public static class ConvoUtilsGPT
{
    public static int GetNotInterestedPromptId() 
        => ClientDataManager.I.SystemPromptNameIdDic[NOT_INTERESTED_PROMPT_NAME];

    public static Prompt CreateNotInterestedPrompt() => new Prompt()
    {
        Id = GetNotInterestedPromptId(),
        Name = NOT_INTERESTED_PROMPT_NAME,
        EndConvoAbility = EndConvoAbility.Always
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

    public static DataRequester ChatGPTResponseRequester = new DataRequester();

    public static int ConvoEndOddsIfSometimes = 50;

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

    public static EnrichedPromptLocText ProduceEnrichedLocText(this PromptLoc promptLoc, EndConvoAbility endConvoAbility, int chanceIfSometimes = 50)
        => promptLoc.Text.ProduceEnrichedLocText(endConvoAbility, chanceIfSometimes);

    public static EnrichedPromptLocText ProduceEnrichedLocText(this string promptText, EndConvoAbility endConvoAbility, int chanceIfSometimes = 50)
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
                    promptText += GetConvoEndInstruction();
                }
                break;
            case EndConvoAbility.Always:
                willBeAbleToEndConvo = true;
                promptText += GetConvoEndInstruction();
                break;
        }

        return new EnrichedPromptLocText
        {
            CanEndConvoThisTime = willBeAbleToEndConvo,
            FullyAssembledText = promptText
        };
    }

    public static EnrichedPromptLocText GetNotInterestedEnrichedText()
    {
        var text = GetPromptTextInCurrentLanguage(GetNotInterestedPromptId());
        return text.ProduceEnrichedLocText(EndConvoAbility.Always);
    }

    public static string GetConvoEndInstruction()
    {
        string localisedText = GetPromptTextInCurrentLanguage(
            promptId: ClientDataUtils.GetSystemPromptId(CAN_END_CONVO_PROMPT_NAME));

        return localisedText.FormatInConvoEndString();
    }

    public static string FormatInConvoEndString(this string localisedText)
        => string.Format(localisedText, CONVO_END_STRING);

    public static void InitNewConvoWithPrompt(string prompt)
    {
        Messages.Clear();
        Messages.Add(new ChatMessage() { Role = "system", Content = prompt });
    }

    public static void GetServerResponseTo(string msgText)
    {
        Debug.Log(msgText);
        var request = messages.GetSerialisedChatRequest(msgText);
        if (request.HasValue)
        {
            chatRequestToProcess = request.Value;
            ChatGPTResponseRequester.RequestData();
        }
        else
        {
            ConversationUIChatGPT.I.SetNewDialogueToDisplay(safetyNetMessage.Content);
            ChatGPTResponseRequester.UpdateDataReceivedAndProcessed();
        }
    }

    public static FixedString4096Bytes? GetSerialisedChatRequest(string msgText)
    {
        if (TryAddUserResponse(msgText))
        {
            Debug.Log("after tryAddUserResponse");
            var serialised = messages.ProduceSerialisedChatRequest();
            if (serialised.Length < 4093)
            {
                return serialised;
            } 
        }
        return null;
    }


    public static FixedString4096Bytes? GetSerialisedChatRequest(this List<OpenAI.ChatMessage> conversationHistory,
        string newMsgText)
    {
        if (conversationHistory.TryAddUserResponse(newMsgText))
        {
            Debug.Log("after tryAddUserResponse");
            var serialised = conversationHistory.ProduceSerialisedChatRequest();
            if (serialised.Length < 4093)
            {
                return serialised;
            }
        }
        return null;
    }

    public static bool TryAddUserResponse(string msgText)
    {
        return messages.TryAddUserResponse(msgText);
    }

    public static bool TryAddUserResponse(this List<OpenAI.ChatMessage> toMessages, string msgText)
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
            var cutCharCount = userMessage.Content.Length - USER_MSG_CHAR_LIMIT;
            Debug.Log($"Cut {cutCharCount} characters because message was too long");
            userMessage.Content = userMessage.Content.Substring(0, USER_MSG_CHAR_LIMIT);
        }

        toMessages.Add(userMessage);

        return true;
    }

    public static CreateChatCompletionRequest ProduceChatRequest(this List<OpenAI.ChatMessage> withMessages) =>
        new CreateChatCompletionRequest()
        {
            Model = _model,
            Temperature = _temperature,
            MaxTokens = _maxTokens,
            Messages = withMessages,
            FrequencyPenalty = _frequencyPenalty,
            PresencePenalty = _presencePenalty
        };

    public static string ProduceSerialisedChatRequest(this List<ChatMessage> fromMessages)
    {
        try
        {
            var chatRequest = fromMessages.ProduceChatRequest();
            return JsonConvert.SerializeObject(chatRequest);
        }
        catch (Exception)
        {
            return null;
        }
    }

    #region only done on server
    public static async Task<string> GetResponseAsServer(string serialisedChatRequest)
    {
        //ServerSideManagerUI.I.WriteLineToOutput(serialisedChatRequest);
        var chatRequest = JsonConvert.DeserializeObject<CreateChatCompletionRequest>(serialisedChatRequest.ToString());
        //ServerSideManagerUI.I.WriteLineToOutput(chatRequest.ToString());
        try
        {
            var responseMessage = await GetResponseAsServer(chatRequest);
            var serialisedChatMessage = JsonConvert.SerializeObject(responseMessage);
            return serialisedChatMessage;
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            return JsonConvert.SerializeObject(safetyNetMessage);
        }
    }

    public static async Task<ChatMessage> GetResponseAsServer(CreateChatCompletionRequest chatRequest)
    {
        try
        {
            var chatResult = await API.CreateChatCompletion(chatRequest);
            if (chatResult.IsError())
            {
                ServerSideManagerUI.I.WriteBadLineToOutput("ERROR: " + chatResult.Error.Message);
                return safetyNetMessage;
            }

            ChatMessage responseMessage = new ChatMessage()
            {
                Role = chatResult.Choices[0].Message.Role,
                Content = chatResult.Choices[0].Message.Content
            };

            return responseMessage;
        }
        catch (Exception e)
        {
            ServerSideManagerUI.I.WriteBadLineToOutput(e.ToString());
            return safetyNetMessage;
        }
    }
    #endregion only done on server

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
        ChatGPTResponseRequester.UpdateDataReceivedAndProcessed();
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

    public static string GetPromptTextInCurrentLanguage(int promptId)
    {
        if (!ClientDataManager.I.PromptLocTextDic.TryGetValue((promptId, UserSettingsManager.I.ConversationLanguage.Id), out var promptText))
        { 
            promptText = $"Something went wrong getting text for promptId {promptId} in language {UserSettingsManager.I.ConversationLanguage.Name}.";
            //TODO: if it wasn't cached, get it from the server
        }
        return promptText;
    }
}
