using Assets.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class Utilities
{
    public static ConversationModes ConversationMode = ConversationModes.RealGPT;

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

    public static bool MakeSureFileExists(string dirPath, string fileName)
    {
        var filePath = Path.Combine(dirPath, fileName);
        try
        {
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }

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

    public static List<TEnum> ValueList<TEnum>() => Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
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
