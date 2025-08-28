using Assets.Classes;
using System.Collections;
using UnityEngine;

public class PromptTestingManager : MonoBehaviour
{
    public static PromptTestingManager I => instance;
    static PromptTestingManager instance;

    private string currentlyTestPromptName;

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
        if (TestingUtilsGPT.Outreacher.NeedsData())
        {
            TestingUtilsGPT.Outreacher.UpdateBeganProcessing();
            GetParticipantResponse(TestingUtilsGPT.Outreacher);
        }

        if (TestingUtilsGPT.Passerby.NeedsData())
        {
            TestingUtilsGPT.Passerby.UpdateBeganProcessing();
            GetParticipantResponse(TestingUtilsGPT.Passerby);
        }
    }

    private async void GetParticipantResponse(TestingConvoParticipant participant)
    {
        var res = await ConvoUtilsGPT.GetResponseAsServer(participant.GetChatRequestToProcess());
        participant.ReceiveResponse(res);
    }

    public void StartTestingPrompt(string promptName)
    {
        currentlyTestPromptName = promptName;
        TestingUtilsGPT.StartConversationAsOutreacher();
        StartCoroutine(TestPrompt());
    }

    private IEnumerator TestPrompt()
    {
        while (TestingUtilsGPT.TestingInProgress)
        {
            yield return OneConversationExchange();
        }

        ServerSideManagerUI.I.WriteCyanLineToOutput("Finished testing the prompt.");

        TestingUtilsGPT.ExportConversationToFile(TestingUtilsGPT.Passerby,
            Constants.TestConvoOutputDir, currentlyTestPromptName + Utils.GetNowFileTimestamp() + ".txt");

        ServerEditPromptModal.I.SetTestButtonActive(true);
    }

    private IEnumerator OneConversationExchange()
    {
        yield return new WaitWhile(TestingUtilsGPT.Outreacher.IsCurrentlyWaiting);

        var outreacherSaid = TestingUtilsGPT.Outreacher.GetLastMessageInConvo();
        ServerSideManagerUI.I.WriteLineToOutput("outreacher said: " + outreacherSaid.Content);
        TestingUtilsGPT.TestingInProgress = !outreacherSaid.Content.WillEndConvo(out _);

        if (TestingUtilsGPT.TestingInProgress)
        {
            TestingUtilsGPT.Passerby.RequestResponseTo(outreacherSaid.Content);
            yield return new WaitWhile(TestingUtilsGPT.Passerby.IsCurrentlyWaiting);
            var passerbySaid = TestingUtilsGPT.Passerby.GetLastMessageInConvo();
            ServerSideManagerUI.I.WriteLineToOutput("passerby said: " + passerbySaid.Content);
            TestingUtilsGPT.TestingInProgress = !passerbySaid.Content.WillEndConvo(out _);

            if (TestingUtilsGPT.TestingInProgress)
            {
                TestingUtilsGPT.Outreacher.RequestResponseTo(passerbySaid.Content);
            }
        }
    }

    private IEnumerator OutreacherSide()
    {
        yield return new WaitWhile(TestingUtilsGPT.Outreacher.IsCurrentlyWaiting);

        var outreacherSaid = TestingUtilsGPT.Outreacher.GetLastMessageInConvo();
        TestingUtilsGPT.TestingInProgress = !outreacherSaid.Content.WillEndConvo(out _);

        if (TestingUtilsGPT.TestingInProgress)
        {
            TestingUtilsGPT.Passerby.RequestResponseTo(outreacherSaid.Content);
        }
    }
    private IEnumerator PasserbySide()
    {
        yield return new WaitWhile(TestingUtilsGPT.Passerby.IsCurrentlyWaiting);

        var passerbySaid = TestingUtilsGPT.Outreacher.GetLastMessageInConvo();
        TestingUtilsGPT.TestingInProgress = !passerbySaid.Content.WillEndConvo(out _);
        if (TestingUtilsGPT.TestingInProgress)
        {
            TestingUtilsGPT.Passerby.RequestResponseTo(passerbySaid.Content);
        }
    }
}
