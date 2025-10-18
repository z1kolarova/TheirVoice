using Assets.Enums;
using Assets.Interfaces;
using SQLite4Unity3d;

public class Prompt : IHasPrimaryKey
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    public EndConvoAbility EndConvoAbility { get; set; }
    public bool ActiveIfAvailable {  get; set; }

    public object GetPrimaryKey()
        => Id;
    public override string ToString()
        => $"[{nameof(Prompt)}: {nameof(Id)}={Id}" +
            $", {nameof(Name)}={Name}" +
            $", {nameof(EndConvoAbility)}={EndConvoAbility}" +
            $", {nameof(ActiveIfAvailable)}={ActiveIfAvailable}" +
            $"]";

    public bool IsSystemPrompt => Name.StartsWith("_");
}