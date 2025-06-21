using SQLite4Unity3d;

public class Language
{

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }

    public override string ToString()
    {
        return string.Format("[Language: Id={0}, Name={1}]", Id, Name);
    }
}