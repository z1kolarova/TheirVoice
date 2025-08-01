using Assets.Enums;
using System.Collections.Generic;

namespace Assets.Classes
{
    public class OriginalPrompt
    {
        public EndConvoAbility GeneralConvoEndingAbility { get; set; }
        //public int? ChaceToEndConvoAutonomously {  get; set; } //null ... use default value
        public bool CanEndConvoThisTime { get; set; }
        public string Text { get; set; }
    }

    //public class CompletePrompt
    //{
    //    string fileName;
    //    string language;
    //    string promptText;
    //    EndConvoAbility endConvoAbility;
    //    List<string> tags;
    //}
}
