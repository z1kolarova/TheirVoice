using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationUI : MonoBehaviour
{
    public static ConversationUI I => instance;
    static ConversationUI instance;

    [SerializeField] List<Button> optionButtons;
    [SerializeField] List<TMP_Text> optionTextMeshes;
    [SerializeField] Button endConversationBtn;

    [SerializeField] Animator npcSpeechBubbleAnimator;
    [SerializeField] TMP_Text npcTextMesh;
    
    [SerializeField] Image crossHair;

    [SerializeField] private float typingSpeed = 0.05f;

    private Dictionary<Button, PlayerConvoBlock> currentOptions = new Dictionary<Button, PlayerConvoBlock>();
    private float speechBubbleAnimationDelay = 0.6f;

    private bool speechBubbleOpen = false;

    public void Start()
    {
        instance = this;

        foreach (var button in optionButtons)
        {
            currentOptions.Add(button, null);
            button.onClick.AddListener(() =>
            {
                HandleOptionButtonClick(button);
                //foreach (var b in ConversationUI.I.optionButtons)
                //{
                //    b.enabled = false;
                //}
                //I.endConversationBtn.enabled = false;
                //StartCoroutine(ContinueDialogue(currentOptions[button]));
            });
        }

        endConversationBtn.onClick.AddListener(() =>
        {
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    public void StartDialogue(bool npcInterested = true, bool playerStarts = true)
    {
        DisplayUI();
        PopulateOptionButtons(ConversationManager.I.GetFirstPlayerOptions(npcInterested));
    }

    public IEnumerator ContinueDialogue(PlayerConvoBlock conversationBlock)
    {
        Debug.Log($"You chose {conversationBlock.Text}");
        var npcResponseBlock = ConversationManager.I.GetNPCAnswerTo(conversationBlock);
        yield return StartCoroutine(DoNPCDialogue(npcResponseBlock.Text, npcResponseBlock.EndsConvo));
        if (npcResponseBlock.EndsConvo)
        {
        }
        else
        { 
            PopulateOptionButtons(ConversationManager.I.GetPlayerOptionsAfter(npcResponseBlock)); //TODO!!!
        }
    }

    public IEnumerator EndDialogue()
    {
        Debug.Log("4-1) doesn't matter" + speechBubbleOpen);
        if (speechBubbleOpen)
        {
            Debug.Log("4-2) should be open" + speechBubbleOpen);
            yield return StartCoroutine(CloseSpeechBubble());
            Debug.Log("4-3) should be closed" + speechBubbleOpen);
        }
        Debug.Log("4-4");
        ConversationManager.I.TriggerEndDialogue();
        Debug.Log("4-5");
        HideUIAndLockMouse();
        Debug.Log("4-6");
    }

    public void DisplayUI()
    {
        transform.gameObject.SetActive(true);
        crossHair.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator DoNPCDialogue(string text, bool willEndConvo)
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
            Debug.Log("1) should be closed" + speechBubbleOpen);
        }
        Debug.Log("2) should be closed" + speechBubbleOpen);
        yield return StartCoroutine(OpenCleanSpeechBubble());
        Debug.Log("3) should be open" + speechBubbleOpen);
        yield return StartCoroutine(TypeDialogueCoroutine(text));
        if (willEndConvo)
        {
            Debug.Log("4) should be open" + speechBubbleOpen);
            yield return StartCoroutine(EndDialogue());
            Debug.Log("5) should be closed" + speechBubbleOpen);
        }
    }

    private IEnumerator TypeDialogueCoroutine(string sentence)
    {
        foreach (var letter in sentence.ToCharArray())
        {
            npcTextMesh.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator OpenCleanSpeechBubble()
    {
        npcTextMesh.text = string.Empty;
        npcSpeechBubbleAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        speechBubbleOpen = true;
    }
    private IEnumerator CloseSpeechBubble()
    {
        Debug.Log("4-2-1 should be open" + speechBubbleOpen);
        npcSpeechBubbleAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);

        Debug.Log("4-2-2 should be open" + speechBubbleOpen);
        speechBubbleOpen = false;

        Debug.Log("4-2-3 should be closed" + speechBubbleOpen);
    }

    private void CloseOneBubbleOpenAnother()
    {
        StartCoroutine(CloseSpeechBubble());
        StartCoroutine(OpenCleanSpeechBubble());
    }

    public void HideUIAndLockMouse()
    {
        transform.gameObject.SetActive(false);
        crossHair.gameObject.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
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
            StartCoroutine(ContinueDialogue(currentOptions[button]));
        }
    }
}
