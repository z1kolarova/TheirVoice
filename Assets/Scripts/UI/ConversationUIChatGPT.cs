﻿using Assets.Classes;
using Assets.Scripts;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
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

        micBtn.transform.gameObject.SetActive(AudioInputManager.I.MicrophoneSelected);

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

        //var response = realGPT ? ConvoUtilsGPT.GetResponseTo(msgText) : ConvoUtilsGPT.FakeGettingResponseTo(msgText);
        //yield return new WaitUntil(() => response.IsCompleted);
        //yield return StartCoroutine(ContinueDialogue(response.Result));

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
            StartCoroutine(GetAndDisplayResponse(Utilities.ConversationMode == ConversationModes.RealGPT));
        }
    }
}