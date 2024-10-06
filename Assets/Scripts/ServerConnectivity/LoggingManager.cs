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

    private const float LOG_SAVE_INTERVAL = 5f;
    private float timeSinceLastSave = 0f;

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

    private void Update() {
        timeSinceLastSave += Time.deltaTime;
        if (timeSinceLastSave > LOG_SAVE_INTERVAL) {
            timeSinceLastSave = 0f;
            WriteLogFile();
        }
    }

    public void WriteLogFile()
    {
        if (ServerSideManagerUI.I == null)
        {
            return;
        }
        if (Utilities.MakeSureFileExists(logDirPath, currentLogFileName))
        {
            File.WriteAllText(currentLogFilePath, ServerSideManagerUI.I.GetFullLogText());
        }
    }


}
