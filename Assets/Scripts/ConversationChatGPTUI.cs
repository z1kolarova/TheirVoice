using Assets.Classes;
using Assets.Scripts;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationUIChatGPT : MonoBehaviour
{
    public static ConversationUIChatGPT I => instance;
    static ConversationUIChatGPT instance;

    [SerializeField] SpeechBubble npcSpeechBubble;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendBtn;

    [SerializeField] Button endConversationBtn;

    private string prompt = "You are a very friendly 86 years old grandma to 8 grandkids. You love baking cookies and other sweets for them. You were on your way to the shop to buy more baking ingredients when you noticed people with TV screens that play video footage of animals in slaughterhouses. You stopped to watch for a bit and one of them approached you.\r\nTry to mimic a spoken conversation.\r\nKeep your responses short and to the point but also bring up your grandchildren and how much you love them. Don't assume the gender of the person you're responding to.";

    public void Start()
    {
        instance = this;

        sendBtn.onClick.AddListener(() => { 
            StartCoroutine(GetAndDisplayResponse(Utilities.ConversationMode == ConversationModes.RealGPT));
            inputField.Select();
        });

        endConversationBtn.onClick.AddListener(() =>
        {
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    public void StartDialogue(bool npcInterested = true, bool playerStarts = true)
    {
        SpeechBubbleManager.I.SetUsedSpeechBubble(npcSpeechBubble);
        sendBtn.enabled = true;
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
        DisplayUI();
    }

    private IEnumerator GetAndDisplayResponse(bool realGPT)
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            yield break;
        }

        var msgText = inputField.text.Trim();
        sendBtn.enabled = false;
        inputField.text = "";

        var response = realGPT ? ConvoUtilsGPT.GetResponseTo(msgText) : ConvoUtilsGPT.FakeGettingResponseTo(msgText);
        yield return new WaitUntil(() => response.IsCompleted);
        yield return StartCoroutine(ContinueDialogue(response.Result));

        sendBtn.enabled = true;
    }

    //private async void GetResponse()
    //{
    //    if (string.IsNullOrWhiteSpace(inputField.text))
    //    {
    //        return;
    //    }

    //    var msgText = inputField.text.Trim();
    //    sendBtn.enabled = false;
    //    inputField.text = "";

    //    var response = await ConvoUtilsGPT.GetResponseTo(msgText);
    //    StartCoroutine(DoNPCDialogue(response, false));

    //    sendBtn.enabled = true;
    //}

    //private async void FakeGettingResponse()
    //{
    //    if (string.IsNullOrWhiteSpace(inputField.text))
    //    {
    //        return;
    //    }

    //    var msgText = inputField.text.Trim();
    //    sendBtn.enabled = false;
    //    inputField.text = "";

    //    var responseText = await ConvoUtilsGPT.FakeGettingResponseTo(msgText);
    //    StartCoroutine(DoNPCDialogue(responseText, false));

    //    sendBtn.enabled = true;
    //}

    public IEnumerator ContinueDialogue(string npcResponseText)
    {
        var endsConvo = ConvoUtilsGPT.WillEndConvo(npcResponseText);
        yield return StartCoroutine(SpeechBubbleManager.I.DoNPCDialogue(npcResponseText));
        if (endsConvo)
        {
            yield return StartCoroutine(EndDialogue());
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

    private void OnEnable()
    {
        inputField.onEndEdit.AddListener(OnEndEdit);
    }
    private void OnDisable()
    {
        inputField.onEndEdit.RemoveListener(OnEndEdit);
    }
    private void OnEndEdit(string inputString)
    {
        if (string.IsNullOrWhiteSpace(inputString))
            return;

        if (Input.GetKeyDown(KeyCode.Return) && sendBtn.isActiveAndEnabled)
        {
            StartCoroutine(GetAndDisplayResponse(Utilities.ConversationMode == ConversationModes.RealGPT));
        }
    }
}
