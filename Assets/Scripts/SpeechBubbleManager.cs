using System.Collections;
using TMPro;
using UnityEngine;

public class SpeechBubbleManager : MonoBehaviour
{
    public static SpeechBubbleManager I => instance;
    static SpeechBubbleManager instance;

    private Animator npcSpeechBubbleAnimator;
    [SerializeField] Transform npcSpeechBubble;
    [SerializeField] TMP_Text npcTextMesh;

    [SerializeField] private float TYPING_SPEED = 0.05f;
    
    private const float SPEECH_BUBBLE_ANIMATION_DELAY = 0.6f;
    private bool speechBubbleOpen = false;

    private void Start()
    {
        instance = this;
    }

    public void SetUsedSpeechBubble(SpeechBubble bubbleToUse)
    {
        npcSpeechBubble = bubbleToUse.physicalRepresentation;
        npcSpeechBubble.localScale = Vector3.zero;
        npcSpeechBubbleAnimator = bubbleToUse.animator;
        speechBubbleOpen = false;
        npcTextMesh = bubbleToUse.textMesh;
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
            yield return new WaitForSeconds(TYPING_SPEED);
        }
    }

    private IEnumerator OpenCleanSpeechBubble()
    {
        npcTextMesh.text = string.Empty;
        npcSpeechBubbleAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(SPEECH_BUBBLE_ANIMATION_DELAY);
        speechBubbleOpen = true;
    }
    private IEnumerator CloseSpeechBubble()
    {
        npcSpeechBubbleAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(SPEECH_BUBBLE_ANIMATION_DELAY);

        speechBubbleOpen = false;
    }
    public IEnumerator EndOfDialogue()
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
        }
    }
}
