using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationOptionsDisplay : MonoBehaviour
{
    public static ConversationOptionsDisplay I => instance;
    static ConversationOptionsDisplay instance;

    [SerializeField] List<Button> optionButtons;
    [SerializeField] List<TMP_Text> optionTextMeshes;
    [SerializeField] Button endConversationBtn;

    [SerializeField] Animator npcSpeechBubbleAnimator;
    [SerializeField] TMP_Text npcTextMesh;
    
    [SerializeField] Image crossHair;

    [SerializeField] private float typingSpeed = 0.05f;

    private Dictionary<Button, ConversationBlock> currentOptions = new Dictionary<Button, ConversationBlock>();
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
                foreach (var b in ConversationOptionsDisplay.I.optionButtons)
                {
                    b.enabled = false;
                }
                I.endConversationBtn.enabled = false;
                StartCoroutine(ContinueDialogue(currentOptions[button]));
            });
        }

        endConversationBtn.onClick.AddListener(() =>
        {
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    public void StartDialogue(bool playerStarts = true)
    {
        DisplayUI();
        PopulateOptionButtons(ConversationConsts.P_OpeningLines);
    }

    public IEnumerator ContinueDialogue(ConversationBlock conversationBlock)
    {
        Debug.Log($"You chose {conversationBlock.Text}");
        var npcResponse = ConversationManager.I.GetResponseTo(conversationBlock);
        yield return StartCoroutine(DoNPCDialogue(npcResponse));
        PopulateOptionButtons(ConversationConsts.P_TestingSet);
    }

    public IEnumerator EndDialogue()
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
        }
        ConversationManager.I.TriggerEndDialogue();
        HideUIAndLockMouse();
    }

    public void DisplayUI()
    {
        transform.gameObject.SetActive(true);
        crossHair.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator DoNPCDialogue(string text)
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
        }
        yield return StartCoroutine(OpenCleanSpeechBubble());
        yield return StartCoroutine(TypeDialogueCoroutine(text));
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
        npcSpeechBubbleAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        speechBubbleOpen = false;
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

    private void PopulateOptionButtons(List<ConversationBlock> conversationBlocks)
    {
        for (int i = 0; i < optionTextMeshes.Count && i < optionButtons.Count; i++)
        {
            optionTextMeshes[i].text = conversationBlocks[i].Text;
            currentOptions[optionButtons[i]] = conversationBlocks[i];
        }
        foreach (var b in ConversationOptionsDisplay.I.optionButtons)
        {
            b.enabled = true;
        }
        I.endConversationBtn.enabled = true;
    }
}
