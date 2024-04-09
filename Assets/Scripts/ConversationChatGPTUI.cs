using Assets.Classes;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ConversationUIChatGPT : MonoBehaviour
{
    public static ConversationUIChatGPT I => instance;
    static ConversationUIChatGPT instance;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendBtn;
    [SerializeField] Button endConversationBtn;

    [SerializeField] Animator npcSpeechBubbleAnimator;
    [SerializeField] TMP_Text npcTextMesh;

    [SerializeField] Image crossHair;

    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private int userMsgCharLimit = 500;

    private float speechBubbleAnimationDelay = 0.6f;

    private bool speechBubbleOpen = false;

    private string prompt = "You are a very friendly 86 years old grandma to 8 grandkids. You love baking cookies and other sweets for them. You were on your way to the shop to buy more baking ingredients when you noticed people with TV screens that play video footage of animals in slaughterhouses. You stopped to watch for a bit and one of them approached you.\r\nTry to mimic a spoken conversation.\r\nKeep your responses short and to the point but also bring up your grandchildren and how much you love them. Don't assume the gender of the person you're responding to.";

    private OpenAIAPI api;
    private List<ChatMessage> messages;

    public void Start()
    {
        instance = this;
        api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY_THEIR_VOICE", EnvironmentVariableTarget.User));

        sendBtn.onClick.AddListener(() => {
            if (ConvoUtils.Mode == ConversationModes.RealGPT)
            {
                GetResponse();
            }
            else
            {
                FakeGettingResponse();
            }
        });

        endConversationBtn.onClick.AddListener(() =>
        {
            StartCoroutine(EndDialogue());
        });

        HideUIAndLockMouse();
    }

    private void Init()
    {
        messages = new List<ChatMessage> { 
            new ChatMessage(ChatMessageRole.System,
            prompt)
        };
        inputField.text = "";
        npcTextMesh.text = "";
    }

    private async void GetResponse()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            return;
        }

        sendBtn.enabled = false;

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text.Trim();

        if (userMessage.Content.Length > userMsgCharLimit)
        {
            userMessage.Content = userMessage.Content.Substring(0, userMsgCharLimit);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        messages.Add(userMessage);

        inputField.text = "";

        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest() { 
            Model = Model.ChatGPTTurbo,
            Temperature = 0.1, 
            MaxTokens = 50, 
            Messages = messages
        });

        if (chatResult == null || chatResult.Choices.Count == 0)
        {
            Debug.Log("ChatGPT didn't give back chatResult");
        }
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;

        messages.Add(responseMessage);

        Debug.Log(responseMessage.Content);
        StartCoroutine(DoNPCDialogue(responseMessage.Content, false));

        sendBtn.enabled = true;
    }

    private async void FakeGettingResponse()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
        {
            return;
        }

        sendBtn.enabled = false;

        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text.Trim();

        if (userMessage.Content.Length > userMsgCharLimit)
        {
            userMessage.Content = userMessage.Content.Substring(0, userMsgCharLimit);
        }

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content));

        messages.Add(userMessage);

        inputField.text = "";

        var responseMessage = new ChatMessage(ChatMessageRole.Assistant, "This is a very profound answer to what you just said.");

        messages.Add(responseMessage);

        Debug.Log(responseMessage.Content);
        StartCoroutine(DoNPCDialogue(responseMessage.Content, false));

        sendBtn.enabled = true;
    }

    public void StartDialogue(bool npcInterested = true, bool playerStarts = true)
    {
        if (npcInterested)
        {
            Init();
            DisplayUI();
        }
    }

    public IEnumerator ContinueDialogue(string playerInput)
    {
        var npcResponseText = "";
        var npcEndsConvo = false;
        yield return StartCoroutine(DoNPCDialogue(npcResponseText, npcEndsConvo));
    }

    public IEnumerator EndDialogue()
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
        }
        ConversationManager.I.TriggerEndDialogue();
        HideUIAndLockMouse();
    }

    public void DisplayUI()
    {
        transform.gameObject.SetActive(true);
        crossHair.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator DoNPCDialogue(string text, bool willEndConvo)
    {
        if (speechBubbleOpen)
        {
            yield return StartCoroutine(CloseSpeechBubble());
        }
        yield return StartCoroutine(OpenCleanSpeechBubble());
        yield return StartCoroutine(TypeDialogueCoroutine(text));
        if (willEndConvo)
        {
            yield return StartCoroutine(EndDialogue());
        }
    }

    private IEnumerator TypeDialogueCoroutine(string sentence)
    {
        foreach (var letter in sentence.ToCharArray())
        {
            npcTextMesh.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator OpenCleanSpeechBubble()
    {
        npcTextMesh.text = string.Empty;
        npcSpeechBubbleAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);
        speechBubbleOpen = true;
    }
    private IEnumerator CloseSpeechBubble()
    {
        npcSpeechBubbleAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(speechBubbleAnimationDelay);

        speechBubbleOpen = false;
    }

    private void CloseOneBubbleOpenAnother()
    {
        StartCoroutine(CloseSpeechBubble());
        StartCoroutine(OpenCleanSpeechBubble());
    }

    public void HideUIAndLockMouse()
    {
        transform.gameObject.SetActive(false);
        crossHair.gameObject.SetActive(true);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
