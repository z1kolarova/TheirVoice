using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager I => instance;
    static LanguageManager instance;

    private Dictionary<string, int> langIdDic = null;
    public Dictionary<string, int> LangIdDic
    {
        get
        {
            if (langIdDic == null)
            {
                langIdDic = DBService.I.Languages.ToDictionary(l => l.Name, l => l.Id);
            }
            return langIdDic;
        }
    }
    
    private List<string> languageNames = null;
    public List<string> LanguageNames
    {
        get
        {
            if (languageNames == null)
            {
                languageNames = LangIdDic.Keys.ToList();
                languageNames.Sort();
            }
            return languageNames;
        }
    }

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
}
