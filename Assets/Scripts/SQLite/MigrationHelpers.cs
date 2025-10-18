using SQLite4Unity3d;

namespace Assets.Scripts.SQLite
{
    public static class MigrationHelpers
    {
        public static void CreateOrReplaceTrigger(this SQLiteConnection db, string triggerName, string createSql)
        {
            db.Execute($"DROP TRIGGER IF EXISTS {triggerName};");
            db.Execute(createSql);
        }

        public static void CreateOrReplaceView(this SQLiteConnection db, string viewName, string createSql)
        {
            db.Execute($"DROP VIEW IF EXISTS {viewName};");
            db.Execute(createSql);
        }

        public static void CreateOrReplaceIndex(this SQLiteConnection db, string indexName, string createSql)
        {
            db.Execute($"DROP INDEX IF EXISTS {indexName};");
            db.Execute(createSql);
        }
    }
}
