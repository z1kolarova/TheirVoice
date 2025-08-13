using UnityEngine;

public class UserSettingsManager : MonoBehaviour
{
    public static UserSettingsManager I => instance;
    static UserSettingsManager instance;

    private Language conversationLanguage;
    public Language ConversationLanguage { get { return conversationLanguage; } }

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

    public void SetConversationLanguage(Language language)
    { 
        conversationLanguage = language; 
    }
}
