using Assets.Classes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationOptionsDisplay : MonoBehaviour
{
    [SerializeField] List<Button> optionButtons;
    [SerializeField] List<TMP_Text> optionTexts;
    [SerializeField] Button endConversationBtn;

    private Dictionary<Button, ConversationBlock> currentOptions = new Dictionary<Button, ConversationBlock>();
    public void Start()
    {
        foreach (var button in optionButtons)
        {
            currentOptions.Add(button, null);
            button.onClick.AddListener(() =>
            {
                Debug.Log($"You chose {currentOptions[button].Text}");
            });
        }
        endConversationBtn.onClick.AddListener(() =>
        {
            ConversationManager.I.TriggerEndDialogue();
            HideConversationUIAndLockMouse();
        });

        HideConversationUIAndLockMouse();
    }
    public void PopulateOptionsAndDisplay(List<ConversationBlock> conversationBlocks)
    {
        for (int i = 0; i < optionTexts.Count && i < optionButtons.Count; i++)
        {
            optionTexts[i].text = conversationBlocks[i].Text;
            currentOptions[optionButtons[i]] = conversationBlocks[i];
        }

        //SetOptionBtnsActive(true);

        ShowConversationUIAndUnlockMouse();
    }

    //public void SetOptionBtnsActive(bool active)
    //{
    //    foreach (var btn in optionButtons)
    //    {
    //        GameObject.Find(btn.name).SetActive(active);
    //    }
    //}

    public void ShowConversationUIAndUnlockMouse()
    {
        transform.gameObject.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }
    public void HideConversationUIAndLockMouse()
    {
        transform.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
