using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;

    [SerializeField] private float typingSpeed = 0.05f;

    private float speechBubbleAnimationDelay = 0.6f;
    private PasserbyAI? talkingTo;

    [SerializeField] private ConversationOptionsDisplay cod;

    private void Start()
    {
        instance = this;
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        talkingTo = passerby;
        cod.StartDialogue();
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
