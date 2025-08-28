using Assets.Enums;
using Newtonsoft.Json;
using OpenAI;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public static class Utils
{
    public static ConversationModes ConversationMode = ConversationModes.RealGPT;

    #region Serialization

    private static JsonSerializer serializer;
    public static JsonSerializer Serializer
    {
        get
        {
            if (serializer == null)
            {
                serializer = new JsonSerializer();
            }
            return serializer;
        }
    }

    #endregion Serialization

    #region Files

    public static string OpenFolderDialog(string title, string directory = "")
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel(title, directory, false);
        return paths.Length > 0 ? paths[0] : null;
    }

    public static void EnsureDirectoryExists(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }

    public static string EnsureFileExists(string dirPath, string fileName)
    {
        var filePath = Path.Combine(dirPath, fileName);
        try
        {
            if (!File.Exists(filePath))
            {
                EnsureDirectoryExists(dirPath);
                File.Create(filePath).Close();
            }
            return filePath;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return "";
        }
    }

    public static void WriteFileContents(string dirPath, string fileName, string fileContents)
    {
        var filePath = EnsureFileExists(dirPath, fileName);
        File.WriteAllText(filePath, fileContents);
    }

    public static string GetNowFileTimestamp() => DateTime.Now.ToString("yyyyMMdd-HHmm");

    #endregion Files

    #region Borders and vectors

    public static Borders Borders => new Borders(-25f, 25f, -25f, 25f);
    public static Vector2 ProjectInto(this Vector2 v2, Borders borders)
    {
        var x = v2.x < 0 ? v2.x * borders.XNeg * (-1) : v2.x * borders.XPos;
        var z = v2.y < 0 ? v2.y * borders.ZNeg * (-1) : v2.y * borders.ZPos;
        return new Vector2(x, z);

    }

    public static string ForLogging(this Vector3 v3) => $"{v3.x}, {v3.y}, {v3.z}";
    public static string ForLogging(this Vector2 v2) => $"{v2.x}, {v2.y}";

    public static bool IsNullOrBegining(this Vector3 v3)
        => v3 == null || v3.Equals(Vector3.zero);
    public static bool IsApproximately(this Vector3 v1, Vector3 v2, float precision = 2f, bool ignoreY = true)
    {
        return Mathf.Abs(v1.x - v2.x) <= precision
            && (ignoreY || Mathf.Abs(v1.y - v2.y) <= precision)
            && Mathf.Abs(v1.z - v2.z) <= precision;
    }

    #endregion Borders and vectors

    public static List<TEnum> ValueList<TEnum>() => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();

    #region Dropdowns

    public static string GetDisplayedTextOfDropdown(this TMP_Dropdown dropdown)
        => dropdown.options[dropdown.value].text;

    public static void PopulateDropdownAndPreselect(this TMP_Dropdown dropdown, IEnumerable<string> options,
        string selectedOptionText = "")
    {
        dropdown.options.Clear();

        foreach (var option in options)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }

        dropdown.SelectLabelInDropdown(selectedOptionText);
    }

    public static void SelectLabelInDropdown(this TMP_Dropdown dropdown, string label)
    {
        var labels = dropdown.options.Select(x => x.text).ToList();
        dropdown.value = labels.IndexOf(label);
        dropdown.RefreshShownValue();
    }

    #endregion Dropdowns

    #region Yes and no
    public static IEnumerable<string> NoYesSelectOptions => new string[] { "No", "Yes" };
    public static string YesOrNo(this bool isItYes) => isItYes ? "Yes" : "No";
    public static bool IsYes(this string yesOrNo) => yesOrNo == "Yes";
    #endregion Yes and no

    #region Chunking
    public static void ChunkData(string textData, ref List<string> chunksToProcess, int maxChunkSize = 4000)
    {
        var utf8Bytes = Encoding.UTF8.GetBytes(textData);
        ChunkData(utf8Bytes, ref chunksToProcess, maxChunkSize);
    }
    public static void ChunkData(byte[] utf8Bytes, ref List<string> chunksLocation, int maxChunkSize = 4000)
    {
        chunksLocation.Clear();
        for (int i = 0; i < utf8Bytes.Length; i += maxChunkSize)
        {
            int length = Math.Min(maxChunkSize, utf8Bytes.Length - i);
            byte[] chunk = new byte[length];
            Array.Copy(utf8Bytes, i, chunk, 0, length);
            chunksLocation.Add(Convert.ToBase64String(chunk));
        }
    }
    public static string DechunkStringChunks(this List<string> chunks)
    {
        List<byte> dataAsBytes = new List<byte>();
        for (int i = 0; i < chunks.Count; i++)
        {
            byte[] chunk = Convert.FromBase64String(chunks[i]);
            dataAsBytes.AddRange(chunk);
        }
        return System.Text.Encoding.UTF8.GetString(dataAsBytes.ToArray());
    }
    #endregion Chunking

    #region API
    public static bool IsError(this CreateChatCompletionResponse response)
    {
        return response.Error != null;
    }

    public static string TranscribeConversation(this List<ChatMessage> conversation, bool assistantIsPasserby = true)
    {
        if (conversation == null || conversation.Count == 0)
            return "";

        var transcript = $"{conversation[0].Role.ReplaceRoleName(assistantIsPasserby)}: \n" +
            $"{conversation[0].Content}\n" +
            $"=========================\n";

        for (int i = 1; i < conversation.Count; i++)
        {
            transcript += $"{conversation[i].Role.ReplaceRoleName(assistantIsPasserby)}: " +
            $"{conversation[i].Content}\n\n";
        }

        return transcript;
    }

    public static string ReplaceRoleName(this string originalRoleName, bool assistantIsPasserby = true)
    { 
        return originalRoleName switch
        {
            "assistant" => assistantIsPasserby ? "passerby" : "outreacher",
            "user" => assistantIsPasserby ? "outreacher" : "passerby",
            _ => originalRoleName
        };
    }
    #endregion API
}

public struct Borders {
    public float XNeg;
    public float XPos;
    public float ZNeg;
    public float ZPos;

    public Borders(float xneg, float xpos, float zneg, float zpos)
    {
        XNeg = xneg;
        XPos = xpos;
        ZNeg = zneg;
        ZPos = zpos;
    }
}
