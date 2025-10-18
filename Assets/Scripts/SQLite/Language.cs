using Assets.Interfaces;
using SQLite4Unity3d;

public class Language : IHasPrimaryKey
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }

    public object GetPrimaryKey()
        => Id;
    public override string ToString()
        => $"[{nameof(Language)}: {nameof(Id)}={Id}" +
            $", {nameof(Name)}={Name}" +
            $"]";
}