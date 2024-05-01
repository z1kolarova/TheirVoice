using Assets.Classes;
using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationUI : MonoBehaviour
{
    public static ConversationUI I => instance;
    static ConversationUI instance;

    [SerializeField] SpeechBubble npcSpeechBubble;

    [SerializeField] List<Button> optionButtons;
    [SerializeField] List<TMP_Text> optionTextMeshes;
    private Dictionary<Button, PlayerConvoBlock> currentOptions = new Dictionary<Button, PlayerConvoBlock>();

    [SerializeField] Button endConversationBtn;

    public void Start()
    {
        instance = this;

        foreach (var button in optionButtons)
        {
            currentOptions.Add(button, null);
            button.onClick.AddListener(() =>
            {
                HandleOptionButtonClick(button);
            });
        }

        endConversationBtn.onClick.AddListener(() =>
        {
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    public void StartDialogue(bool npcInterested = true, bool playerStarts = true) {
        SpeechBubbleManager.I.SetUsedSpeechBubble(npcSpeechBubble);
        DisplayUI();
        PopulateOptionButtons(ConversationManager.I.GetFirstPlayerOptions(npcInterested));
    }

    public IEnumerator ContinueDialogue(NPCConvoBlock npcConvoBlock)
    {
        yield return StartCoroutine(SpeechBubbleManager.I.DoNPCDialogue(npcConvoBlock.Text));
        if (npcConvoBlock.EndsConvo)
        {
            //TODO! NPC "ends" the conversation, maybe this is enough?
            yield return StartCoroutine(EndDialogue());
        }
        else
        { 
            PopulateOptionButtons(ConversationManager.I.GetPlayerOptionsAfter(npcConvoBlock)); //TODO!!!
        }
    }

    public IEnumerator EndDialogue()
    {
        yield return StartCoroutine(SpeechBubbleManager.I.EndOfDialogue());
        ConversationManager.I.TriggerEndDialogue();

        HideUIAndLockMouse();
    }

    public void DisplayUI()
    {
        transform.gameObject.SetActive(true);
        UserInterfaceUtilities.I.SetCursorUnlockState(true);
    }

    public void HideUIAndLockMouse()
    {
        transform.gameObject.SetActive(false);
        UserInterfaceUtilities.I.SetCursorUnlockState(false);
    }

    private void PopulateOptionButtons(List<PlayerConvoBlock> conversationBlocks)
    {
        for (int i = 0; i < optionTextMeshes.Count && i < optionButtons.Count && i < conversationBlocks.Count; i++)
        {
            optionTextMeshes[i].text = conversationBlocks[i].Text;
            currentOptions[optionButtons[i]] = conversationBlocks[i];
        }
        foreach (var b in ConversationUI.I.optionButtons)
        {
            b.enabled = true;
        }
        I.endConversationBtn.enabled = true;
    }

    private void HandleOptionButtonClick(Button button)
    {
        if (currentOptions[button].EndsConvo)
        {
            StartCoroutine(EndDialogue());
        }
        else
        {
            foreach (var b in ConversationUI.I.optionButtons)
            {
                b.enabled = false;
            }
            I.endConversationBtn.enabled = false;
            var npcResponseBlock = ConversationManager.I.GetNPCAnswerTo(currentOptions[button]);
            StartCoroutine(ContinueDialogue(npcResponseBlock));
        }
    }
}
