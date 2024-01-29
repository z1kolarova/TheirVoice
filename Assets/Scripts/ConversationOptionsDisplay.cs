using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationOptionsDisplay : MonoBehaviour
{
    [SerializeField] List<Button> optionButtons;
    [SerializeField] List<TMP_Text> optionTextMeshes;
    [SerializeField] Button endConversationBtn;

    [SerializeField] Animator npcSpeechBubbleAnimator;
    [SerializeField] TMP_Text npcTextMesh;

    [SerializeField] private float typingSpeed = 0.05f;

    private Dictionary<Button, ConversationBlock> currentOptions = new Dictionary<Button, ConversationBlock>();
    private float speechBubbleAnimationDelay = 0.6f;

    public void Start()
    {
        //currentOptions.Clear();
        foreach (var button in optionButtons)
        {
            currentOptions.Add(button, null);
            button.onClick.AddListener(() =>
            {
                Debug.Log($"You chose {currentOptions[button].Text}");
                DoNPCDialogue("Yeah, I love animals and this is terrible.");
            });
        }
        endConversationBtn.onClick.AddListener(() =>
        {
            ConversationManager.I.TriggerEndDialogue();
            HideUIAndLockMouse();
        });

        HideUIAndLockMouse();
    }

    public void StartDialogue(bool playerStarts = true)
    {
        DisplayUI();
        PopulateOptionButtons(ConversationConsts.openingLines);
    }

    public void DisplayUI()
    {
        transform.gameObject.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    public void DoNPCDialogue(string text)
    {
        StartCoroutine(OpenCleanSpeechBubble());
        TypeDialogue(text);
    }

    private async void TypeDialogue(string sentence)
    {
        await Task.Delay((int)(speechBubbleAnimationDelay * 1000));
        foreach (var letter in sentence.ToCharArray())
        {
            npcTextMesh.text += letter;
            await Task.Delay((int)(typingSpeed * 1000));
        }
    }

    private IEnumerator OpenCleanSpeechBubble()
    {
        npcTextMesh.text = string.Empty;
        npcSpeechBubbleAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
    }
    private IEnumerator CloseSpeechBubble()
    {
        npcSpeechBubbleAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
    }

    private void CloseOneBubbleOpenAnother()
    {
        StartCoroutine(CloseSpeechBubble());
        StartCoroutine(OpenCleanSpeechBubble());
    }

    public void HideUIAndLockMouse()
    {
        transform.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    private void PopulateOptionButtons(List<ConversationBlock> conversationBlocks)
    {
        for (int i = 0; i < optionTextMeshes.Count && i < optionButtons.Count; i++)
        {
            optionTextMeshes[i].text = conversationBlocks[i].Text;
            currentOptions[optionButtons[i]] = conversationBlocks[i];
        }
    }
}
