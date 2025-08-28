using Assets.Classes;
using OpenAI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public static class ModerationUtils
{
    private static FixedString4096Bytes inputToModerate = "";
    public static FixedString4096Bytes GetInputToProcess() => inputToModerate;

    public static DataRequester ModerationRequester = new DataRequester();
    public static bool? LastInputPassed = null;

    public static OpenAIApi API => ConvoUtilsGPT.API;

    public static void InitiateModeration(string msgText)
    {
        inputToModerate = msgText;
        ModerationRequester.RequestData();
    }

    public static async Task<bool> PassesModeration(string input)
    {
        var cmr = new CreateModerationRequest() { Input = input };

        var response = await API.CreateModeration(input.CreateModerationRequest());

        ServerSideManagerUI.I.WriteLineToOutputWithColor(response.LogFromFlaggedResult(input),
            color: Color.white);

        if (!response.IsFlagged())
            return true;

        //if (!moderationResult.Results.Any(r => r.Flagged))
        //{
        //    return true;
        //}

        ServerSideManagerUI.I.WriteBadLineToOutput(response.LogFromFlaggedResult(input));
        return false;
    }

    public static void ProcessModerationResult(bool passed)
    {
        LastInputPassed = passed;
        ModerationRequester.UpdateDataReceivedAndProcessed();
    }

    public static CreateModerationRequest CreateModerationRequest(this string input)
        => new CreateModerationRequest()
        {
            Input = input
        };

    public static bool IsFlagged(this CreateModerationResponse response)
        => response.Results.Any(r => r.Flagged);

    public static List<string> FlaggedCategories(this ModerationResult result)
        => result.Categories
            .Where(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

    public static List<string> FlaggedCategories(this CreateModerationResponse response)
        => response.Results
            .SelectMany(r => r.Categories
                .Where(c => c.Value))
            .Select(c => c.Key)
            .ToList();

    public static string LogFromFlaggedResult(this CreateModerationResponse result, string input = "")
    {
        var msg = result.Warning + "\n" + result.Results.Count();
        var results = result.Results;

        int counter = 1;
        foreach (var r in results)
        {
            msg += $"\n{counter}: {r.Flagged} - {string.Join(", ", r.FlaggedCategories())}";
            counter++;
        }
        return msg +
            $"{(string.IsNullOrEmpty(input) ? "" : "in the input:\n" + input)}";
    }
    public static string LogFromFlaggedResult(this OpenAI_API.Moderation.ModerationResult result, string input = "")
    {
        return $"moderation flagged primarily: {result.ToString()}" +
            $"{(string.IsNullOrEmpty(input) ? "" : "\nin the input:\n" + input)}";
    }
}
