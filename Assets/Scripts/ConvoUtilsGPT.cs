using Assets.Classes;
using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    private static int _maxTokens = 50;                 //originally was 50
    private static double _frequencyPenalty = 0.4;      //originally was 0
    private static double _presencePenalty = 0.4;       //originally was 0

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

    public static Prompt ResolveConvoEndingAbility(this Prompt prompt, int chanceIfSometimes)
    {
        switch (prompt.EndConvoAbility)
        {
            case EndConvoAbility.Never:
                break;
            case EndConvoAbility.Sometimes:
                if (RngUtils.RollWithinLimitCheck(chanceIfSometimes))
                {
                    prompt.Text += CONVO_END_INSTRUCTION;
                }
                break;
            case EndConvoAbility.Always:
                prompt.Text += CONVO_END_INSTRUCTION;
                break;
        }
        return prompt;
    }
    //public static Prompt AddAbilityToEndConvo(this Prompt originalPrompt)
    //{ 
    //    originalPrompt.Text += CONVO_END_INSTRUCTION;
    //    return originalPrompt;
    //}

    public static void InitNewConvoWithPrompt(string prompt)
    {
        Messages.Clear(); //does this work?
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

        messages.Add(userMessage);


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

    public static Prompt GetPromptByFileName(string name)
    {
        var path = Path.Combine(PromptsDir, $"{name}.json");
        Prompt result;

        using (StreamReader sr = new StreamReader(path))
        using (JsonReader jr = new JsonTextReader(sr))
        {
            result = Utilities.Serializer.Deserialize<Prompt>(jr);
        }

        return result;
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
