using Assets.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LoggingManager : MonoBehaviour
{
    public static LoggingManager I => instance;
    static LoggingManager instance;
    
    [SerializeField] string logDirPath = "./Logs/";
    private string currentLogFileName;
    private string currentLogFilePath;

    private string CreateLogName() => "log" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".txt";

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentLogFileName = CreateLogName();
        currentLogFilePath = Path.Combine(logDirPath, currentLogFileName);
    }

    public void WriteLineToLog(string lineContent)
    {
        if (Utilities.MakeSureFileExists(logDirPath, currentLogFileName))
        {
            using (StreamWriter sw = File.AppendText(currentLogFilePath))
            {
                sw.WriteLine(lineContent);
            }
        }
    }


}
