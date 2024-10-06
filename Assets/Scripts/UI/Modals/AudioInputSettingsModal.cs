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
    }

    private void PopulateDropdownWithMicOptions()
    {
        microphoneSelectionDropdown.options.Clear();
        microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(""));
        foreach (var device in Microphone.devices)
        {
            microphoneSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(device));
        }

        if (AudioInputManager.I.HasMicrophoneSelected)
        {
            microphoneSelectionDropdown.value = Microphone.devices.ToList().IndexOf(AudioInputManager.I.SelectedMicrophone) + 1;
        }
        else
        {
            microphoneSelectionDropdown.value = 0;
        }

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
        gameObject.SetActive(value);
        if (value)
        {
            UserInterfaceUtilities.I.SetCursorUnlockState(true);
            PopulateDropdownWithMicOptions();
        }
    }
}
