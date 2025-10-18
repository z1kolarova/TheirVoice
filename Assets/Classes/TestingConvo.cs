using Assets.Enums;
using OpenAI;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Classes
{
    public class TestingConvo
    {
        public TestingConvoParticipant Outreacher { get; private set; }
        public TestingConvoParticipant Passerby { get; private set; }

        public string TestedPromptName => Passerby.PromptName;
        public string TestedLanguage = "English";
        public bool TestingInProgress = false;

        public TestingConvo(string passerbyPromptName, EndConvoAbility passerbyGeneralEndConvoAbility,
            string outreacherPromptName, EndConvoAbility outreacherGeneralEndConvoAbility = EndConvoAbility.Always)
        {
            Outreacher = new TestingConvoParticipant(outreacherPromptName, outreacherGeneralEndConvoAbility);
            Passerby = new TestingConvoParticipant(passerbyPromptName, passerbyGeneralEndConvoAbility);
        }

        public void Prepare(string language, string outreacherSystemMessage, string passerbySystemMessage, bool matchesSavedVersion)
        {
            Outreacher.ResetWithSystemMessage(outreacherSystemMessage, true);
            Passerby.ResetWithSystemMessage(passerbySystemMessage, matchesSavedVersion);
            TestedLanguage = language;
        }

        public void StartConversationAsOutreacher()
        {
            Outreacher.RequestResponseTo(TestingUtilsGPT.GetPersonDescribingMessage());
            TestingInProgress = true;
        }
    }
}
