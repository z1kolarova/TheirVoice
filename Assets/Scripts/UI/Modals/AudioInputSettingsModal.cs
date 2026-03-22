using Assets.Scripts;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioInputSettingsModal : MonoBehaviour
{
    [SerializeField] TMP_Text noMicUsedLabel;
    [SerializeField] TMP_Dropdown microphoneSelectionDropdown;
    [SerializeField] Button continueWithoutMicBtn;
    [SerializeField] Button selectMicBtn;

    void Start()
    {
#if UNITY_WEBGL
        gameObject.SetActive(false);
        return;
#else
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

        SetActive(true);
#endif
    }

    private void PopulateDropdownWithMicOptions()
    {
        microphoneSelectionDropdown.options.Clear();
        microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(""));
        microphoneSelectionDropdown.value = 0;

#if !UNITY_WEBGL
        foreach (var device in Microphone.devices)
        {
            microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(device));
        }

        if (AudioInputManager.I.HasMicrophoneSelected)
        {
            microphoneSelectionDropdown.value = Microphone.devices.ToList().IndexOf(AudioInputManager.I.SelectedMicrophone) + 1;
        }
#endif

        microphoneSelectionDropdown.RefreshShownValue();
    }

    private void UseSelectedMicrophoneChoice(bool canUseMic)
    {
        var micToUse = canUseMic ? microphoneSelectionDropdown.options[microphoneSelectionDropdown.value].text : "";
        AudioInputManager.I.SetSelectedMicrophone(micToUse);
        ConversationUIChatGPT.I.SetRecordingButtonActive(AudioInputManager.I.HasMicrophoneSelected);
        noMicUsedLabel.gameObject.SetActive(!AudioInputManager.I.HasMicrophoneSelected);
        UserInterfaceUtilities.I.SetCursorUnlockState(false);
    }

    public void SetActive(bool value)
    {
#if UNITY_WEBGL
        gameObject.SetActive(false);
        return;
#else
        gameObject.SetActive(value);
        if (value)
        {
            UserInterfaceUtilities.I.SetCursorUnlockState(true);
            PopulateDropdownWithMicOptions();
        }
#endif
    }
}
