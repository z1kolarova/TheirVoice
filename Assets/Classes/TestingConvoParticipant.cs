using Assets.Enums;
using OpenAI;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Classes
{
    public class TestingConvoParticipant : DataRequester
    {
        private List<ChatMessage> conversationHistory = new List<ChatMessage>();
        public List<ChatMessage> ConversationHistory { get { return conversationHistory; } }

        private CreateChatCompletionRequest chatRequestToProcess;
        public CreateChatCompletionRequest GetChatRequestToProcess() => chatRequestToProcess;

        public readonly string PromptName;
        public readonly EndConvoAbility GeneralEndConvoAbility;
        public readonly bool CanEndConvoThisTime;
        private bool passerbyMatchesTextInDB;
        public bool PasserbyMatchesTextInDB { get { return passerbyMatchesTextInDB; } }
        public TestingConvoParticipant(string promptName, EndConvoAbility generalEndConvoAbility)
        {
            PromptName = promptName;
            GeneralEndConvoAbility = generalEndConvoAbility;
            CanEndConvoThisTime = generalEndConvoAbility.Decide();
        }

        public void ResetWithSystemMessage(string systemMessageContent, bool matchesSavedVersion)
        {
            passerbyMatchesTextInDB = matchesSavedVersion;

            conversationHistory.Clear();
            conversationHistory.Add(new ChatMessage()
            {
                Role = "system",
                Content = systemMessageContent
            });
        }

        //public void ResetWithSystemMessage(ChatMessage systemMessage) 
        //{
        //    conversationHistory.Clear();
        //    conversationHistory.Add(systemMessage);
        //}

        public void RequestResponseTo(string msgText)
        {
            if (conversationHistory.TryAddUserResponse(msgText))
            {
                if (conversationHistory.Count <= 2)
                {
                    ServerSideManagerUI.I.WriteYellowLineToOutput(conversationHistory.TranscribeConversation());
                }
                chatRequestToProcess = conversationHistory.ProduceChatRequest();
                RequestData();
            }
            else
            {
                ServerSideManagerUI.I.WriteBadLineToOutput("conversation history failed to add msgText");
                UpdateDataReceivedAndProcessed();
            }
        }
        public void ReceiveResponse(ChatMessage response)
        {
            conversationHistory.Add(response);
            UpdateDataReceivedAndProcessed();
        }
    
        public ChatMessage GetLastMessageInConvo() => conversationHistory.LastOrDefault();
    }
}
