using Assets.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromptEntry : MonoBehaviour
{
    [SerializeField] Toggle active;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text convoEnding;
    [SerializeField] TMP_Text availableInThisLanguage;
    [SerializeField] Button editPromptBtn;

    private PromptSettingsLabel psl;

    // Start is called before the first frame update
    void Start()
    {
        editPromptBtn.onClick.AddListener(() => {
            Debug.Log("edit prompt button was clicked");
        });

    }

    public void AssignLabel(PromptSettingsLabel promptSettingsLabel)
    {
        psl = promptSettingsLabel;
        active.isOn = psl.Active;
        title.text = psl.Name;
        convoEnding.text = psl.GeneralConvoEndingAbility.ToString();
        availableInThisLanguage.text = promptSettingsLabel.AvailableInCurrentLanguage ? "Yes" : "No";
    }
}
