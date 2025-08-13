using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class APIKeyManager : MonoBehaviour
{
    public static APIKeyManager I => instance;
    static APIKeyManager instance;

    [SerializeField] private string keyPrefix;
    public List<string> KeyNameOptions { get; set; }
    public string SelectedKeyName { get; private set; }
    public void SetSelectedKeyName(string keyName)
    {
        SelectedKeyName = keyName;
        ServerSideManagerUI.I.WriteLineToOutput("Selected API key: " + SelectedKeyName);
    }

    public bool IsKeySelected => !string.IsNullOrEmpty(SelectedKeyName);

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

    public List<string> GetKeyNameOptions()
    {
        if (KeyNameOptions == null || KeyNameOptions.Count == 0)
        {
            LoadKeyNameOptions();
        }
        return KeyNameOptions;
    }

    private void LoadKeyNameOptions()
    {
        KeyNameOptions = Environment.GetEnvironmentVariables()
            .Cast<DictionaryEntry>()
            .Where(de => ((string)de.Key).StartsWith(keyPrefix))
            .Select(de => de.Key.ToString())
            .ToList();
        KeyNameOptions.Sort();
    }
}
