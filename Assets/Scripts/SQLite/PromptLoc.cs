using SQLite4Unity3d;

public class PromptLoc
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PromptId { get; set; }
    public int LangId { get; set; }
    public string Text { get; set; }
    public bool Available {  get; set; }

    public override string ToString()
        => $"[{nameof(PromptLoc)}: {nameof(Id)}={Id}" +
            $", {nameof(PromptId)}={PromptId}" +
            $", {nameof(LangId)}={LangId}" +
            $", {nameof(Available)}={Available}" +
            $"]";
}