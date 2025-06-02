using Assets.Enums;
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
    [SerializeField] Button micBtn;
    [SerializeField] Button sendBtn;

    [SerializeField] Button endConversationBtn;

    private string newDialogueToDisplay;

    public void Start()
    {
        instance = this;

        micBtn.transform.gameObject.SetActive(AudioInputManager.I.HasMicrophoneSelected);

        sendBtn.onClick.AddListener(() => {
            AudioInputManager.I.EnsureRecordingStops();
            StartCoroutine(GetAndDisplayResponse(Utils.ConversationMode == ConversationModes.RealGPT));
            inputField.Select();
        });

        endConversationBtn.onClick.AddListener(() =>
        {
            AudioInputManager.I.EnsureRecordingStops();
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    public void SetRecordingButtonActive(bool active)
    {
        micBtn.transform.gameObject.SetActive(active);
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

    public void SetUserInputText(string transcription)
    {
        inputField.text = transcription;
    }

    public void SetNewDialogueToDisplay(string text)
    {
        newDialogueToDisplay = text;
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

        if (realGPT)
        {
            ConvoUtilsGPT.GetServerResponseTo(msgText);
            yield return new WaitWhile(ConvoUtilsGPT.IsWaitingForResponse);
            yield return StartCoroutine(ContinueDialogue(newDialogueToDisplay));
            sendBtn.enabled = true;

        }
        else
        {
            yield return StartCoroutine(ContinueDialogue("Here's a fake response for you."));
            sendBtn.enabled = true;
        }
    }

    public IEnumerator ContinueDialogue(string npcResponseText)
    {
        var endsConvo = ConvoUtilsGPT.WillEndConvo(npcResponseText, out string npcTextToUse);
        yield return StartCoroutine(SpeechBubbleManager.I.DoNPCDialogue(npcTextToUse));
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
            StartCoroutine(GetAndDisplayResponse(Utils.ConversationMode == ConversationModes.RealGPT));
        }
    }
}
