using Assets.Classes;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager I => instance;
    static ConversationManager instance;

    private PasserbyAI talkingTo;

    private void Start()
    {
        instance = this;
    }

    public void TriggerStartDialogue(PasserbyAI passerby)
    {
        talkingTo = passerby;
        talkingTo.BeApproached(PlayerController.I.transform.gameObject);
        ConversationOptionsDisplay.I.StartDialogue();
    }

    public string GetResponseTo(IConversationBlock conversationBlock)
    {
        return ConversationConsts.RevealDiet(talkingTo.personality);
        //return $"{conversationBlock.Text}? What's that supposed to mean?";
    }

    public void TriggerEndDialogue()
    {
        talkingTo.EndConversation();
        talkingTo = null;

        PlayerController.I.EndConversation();
    }
}
