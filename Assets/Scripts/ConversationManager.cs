using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class ConversationManager : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    //[SerializeField] private bool playerSpeakingFirst;

    //[SerializeField] private TextMeshProUGUI playerDialogueText;
    private TMP_Text npcDialogueText;

    [Header("Animation Controllers")]
    //[SerializeField] private Animator playerSpeechBubbleAnimator;
    private Animator npcSpeechBubbleAnimator;

    //private int playerIndex;
    private int npcIndex;

    private float speechBubbleAnimationDelay = 0.6f;

    private void Start()
    {
    }

    public void TriggerStartDialogue(PasserbyAI passerBy)
    {
        npcSpeechBubbleAnimator = passerBy.speechBubbleAnimator;
        npcDialogueText = passerBy.textMesh;
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
        //if (playerSpeakingFirst)
        //{
        //    StartCoroutine(OpenCleanSpeechBubble(playerSpeechBubbleAnimator, playerDialogueText));
        //    TypePlayerDialogue();
        //}
        //else
        //{
        npcIndex = 0;
        StartCoroutine(OpenCleanSpeechBubble(npcSpeechBubbleAnimator, npcDialogueText));
        TypeNPCDialogue();
        //}
    }

    //private void TypePlayerDialogue()
    //{
    //    //foreach (var letter in playerDialogueSentences[playerIndex].ToCharArray())
    //    //{
    //    //    playerDialogueText.text += letter;
    //    //    yield return new WaitForSeconds(typingSpeed);
    //    //}
    //    StartCoroutine(TypeDialogue(playerDialogueSentences[playerIndex], playerDialogueText));
    //}

    private void TypeNPCDialogue()
    {
        //foreach (var letter in playerDialogueSentences[playerIndex].ToCharArray())
        //{
        //    playerDialogueText.text += letter;
        //    yield return new WaitForSeconds(typingSpeed);
        //}
        TypeDialogue(ConversationConsts.tempDialogSentences[0], npcDialogueText);
    }

    private async void TypeDialogue(string sentence, TMP_Text destination) {
        await Task.Delay((int)(speechBubbleAnimationDelay * 1000));
        foreach (var letter in sentence.ToCharArray())
        {
            destination.text += letter;
            await Task.Delay((int)(typingSpeed * 1000));
        }
    }

    private IEnumerator OpenCleanSpeechBubble(Animator animator, TMP_Text textDestination)
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

    private void CloseOneBubbleOpenAnother(Animator close, Animator open, TMP_Text openTextDestination)
    {
        StartCoroutine(CloseSpeechBubble(close));
        StartCoroutine(OpenCleanSpeechBubble(open, openTextDestination));
    }
}
