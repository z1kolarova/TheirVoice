using Assets.Classes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PromptTestingManager : MonoBehaviour
{
    public static PromptTestingManager I => instance;
    static PromptTestingManager instance;

    private List<TestingConvo> ongoingTests = null;
    public List<TestingConvo> OngoingTests
    {
        get
        {
            if (ongoingTests == null)
            {
                ongoingTests = new List<TestingConvo>();
            }
            return ongoingTests;
        }
    }

    private void Awake()
    {
        if (I == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (OngoingTests.Any(t => t.Outreacher.NeedsData()))
        {
            foreach (var convo in OngoingTests.Where(t => t.Outreacher.NeedsData()))
            {
                convo.Outreacher.UpdateBeganProcessing();
                GetParticipantResponse(convo.Outreacher);
            }
        }

        if (OngoingTests.Any(t => t.Passerby.NeedsData()))
        {
            foreach (var convo in OngoingTests.Where(t => t.Passerby.NeedsData()))
            {
                convo.Passerby.UpdateBeganProcessing();
                GetParticipantResponse(convo.Passerby);
            }
        }
    }

    private async void GetParticipantResponse(TestingConvoParticipant participant)
    {
        var res = await ConvoUtilsGPT.GetResponseAsServer(participant.GetChatRequestToProcess());
        participant.ReceiveResponse(res);
    }

    public void StartTestingConversation(TestingConvo testingConvo)
    {
        testingConvo.StartConversationAsOutreacher();
        OngoingTests.Add(testingConvo);
        StartCoroutine(TestPrompt(testingConvo));
    }

    private IEnumerator TestPrompt(TestingConvo testingConvo)
    {
        while (testingConvo.TestingInProgress)
        {
            yield return OneConversationExchange(testingConvo);
        }

        ServerSideManagerUI.I.WriteCyanLineToOutput($"Finished testing the prompt: {testingConvo.TestedPromptName} ({testingConvo.TestedLanguage})");

        OngoingTests.Remove(testingConvo);
        testingConvo.ExportPasserbyReportFile();

        ServerEditPromptModal.I.ReflectRunningTestsInUI();
    }

    private IEnumerator OneConversationExchange(TestingConvo testingConvo)
    {
        yield return new WaitWhile(testingConvo.Outreacher.IsCurrentlyWaiting);

        var outreacherSaid = testingConvo.Outreacher.GetLastMessageInConvo();
        ServerSideManagerUI.I.WriteLineToOutput("outreacher said: " + outreacherSaid.Content);
        testingConvo.TestingInProgress = !outreacherSaid.Content.WillEndConvo(out _);

        if (testingConvo.TestingInProgress)
        {
            testingConvo.Passerby.RequestResponseTo(outreacherSaid.Content);
            yield return new WaitWhile(testingConvo.Passerby.IsCurrentlyWaiting);
            var passerbySaid = testingConvo.Passerby.GetLastMessageInConvo();
            ServerSideManagerUI.I.WriteLineToOutput("passerby said: " + passerbySaid.Content);
            testingConvo.TestingInProgress = !passerbySaid.Content.WillEndConvo(out _);

            if (testingConvo.TestingInProgress)
            {
                testingConvo.Outreacher.RequestResponseTo(passerbySaid.Content);
            }
        }
    }
}
