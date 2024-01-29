using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Assets.Classes;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;

    [SerializeField] private float typingSpeed = 0.05f;

    private float speechBubbleAnimationDelay = 0.6f;
    private PasserbyAI? talkingTo;

    private void Start()
    {
        instance = this;
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        talkingTo = passerby;
        ConversationOptionsDisplay.I.StartDialogue();
    }

    public string GetResponseTo(ConversationBlock conversationBlock)
    {
        return $"{conversationBlock.Text}? What's that supposed to mean?";
    }

    public void TriggerEndDialogue()
    {
        talkingTo.EndConversation();
        talkingTo = null;

        PlayerController.I.EndConversation();
    }

    private void Update()
    {

    }

    private async void TypeDialogue(string sentence, TMP_Text destination) {
        await Task.Delay((int)(speechBubbleAnimationDelay * 1000));
        foreach (var letter in sentence.ToCharArray())
        {
            destination.text += letter;
            await Task.Delay((int)(typingSpeed * 1000));
        }
    }
}
