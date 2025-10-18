using Assets.Interfaces;
using SQLite4Unity3d;

public class Meta : IHasPrimaryKey
{
    [PrimaryKey]
    public string Key { get; set; }

    public string Value { get; set; }

    public static string SchemaVersionKey = "schema_version";
    public static string DBBackUpDirKey = "db_backup_dir";

    public object GetPrimaryKey()
        => Key;
    public override string ToString()
        => $"[{nameof(Meta)}: {nameof(Key)}={Key}" +
            $", {nameof(Value)}={Value}" +
            $"]";
}