using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConversationManagerBackup : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool playerSpeakingFirst;

    [SerializeField] private TextMeshProUGUI playerDialogueText;
    [SerializeField] private TextMeshProUGUI npcDialogueText;

    [SerializeField] private string[] playerDialogueSentences;
    [SerializeField] private string[] npcDialogueSentences;

    [Header("Animation Controllers")]
    [SerializeField] private Animator playerSpeechBubbleAnimator;
    [SerializeField] private Animator npcSpeechBubbleAnimator;

    private int playerIndex;
    private int npcIndex;

    private float speechBubbleAnimationDelay = 0.6f;

    private void Start()
    {
    }

    private void TriggerStartDialogue()
    {
        StartDialogue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {

        }
    }

    private void StartDialogue()
    {
        if (playerSpeakingFirst)
        {
            StartCoroutine(OpenCleanSpeechBubble(playerSpeechBubbleAnimator, playerDialogueText));
            TypePlayerDialogue();
        }
        else
        {
            StartCoroutine(OpenCleanSpeechBubble(npcSpeechBubbleAnimator, npcDialogueText));
            TypeNPCDialogue();
        }
    }

    private void TypePlayerDialogue()
    {
        //foreach (var letter in playerDialogueSentences[playerIndex].ToCharArray())
        //{
        //    playerDialogueText.text += letter;
        //    yield return new WaitForSeconds(typingSpeed);
        //}
        StartCoroutine(TypeDialogue(playerDialogueSentences[playerIndex], playerDialogueText));
    }

    private void TypeNPCDialogue()
    {
        //foreach (var letter in playerDialogueSentences[playerIndex].ToCharArray())
        //{
        //    playerDialogueText.text += letter;
        //    yield return new WaitForSeconds(typingSpeed);
        //}
        TypeDialogue(npcDialogueSentences[npcIndex], npcDialogueText);
    }

    private IEnumerator TypeDialogue(string sentence, TextMeshProUGUI destination)
    {
        foreach (var letter in sentence.ToCharArray())
        {
            destination.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator OpenCleanSpeechBubble(Animator animator, TextMeshProUGUI textDestination)
    {
        textDestination.text = string.Empty;
        animator.SetTrigger("Open");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
    }
    private IEnumerator CloseSpeechBubble(Animator animator)
    {
        animator.SetTrigger("Close");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
    }

    private void CloseOneBubbleOpenAnother(Animator close, Animator open, TextMeshProUGUI openTextDestination)
    {
        StartCoroutine(CloseSpeechBubble(close));
        StartCoroutine(OpenCleanSpeechBubble(open, openTextDestination));
    }
}