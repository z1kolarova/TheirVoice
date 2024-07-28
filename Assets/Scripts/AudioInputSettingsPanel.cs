using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioInputSettingsPanel : MonoBehaviour
{
    [SerializeField] TMP_Dropdown microphoneSelectionDropdown;
    [SerializeField] Button continueWithoutMicBtn;
    [SerializeField] Button selectMicBtn;

    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(device));
        }

        continueWithoutMicBtn.onClick.AddListener(() =>
        {
            UseSelectedMicrophoneChoice(false);
            SetActive(false);
        });

        selectMicBtn.onClick.AddListener(() =>
        {
            UseSelectedMicrophoneChoice(true);
            SetActive(false);
        });
    }

    private void UseSelectedMicrophoneChoice(bool canUseMic)
    {
        ConversationUIChatGPT.I.SetRecordingButtonActive(canUseMic);
        var micToUse = canUseMic ? microphoneSelectionDropdown.options[microphoneSelectionDropdown.value].text : "";
        AudioInputManager.I.SetSelectedMicrophone(micToUse);
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
