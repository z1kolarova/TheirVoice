using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DBService
{
    public static DBService I
    {
        get {
            if (instance == null)
            {
                instance = new DBService(Constants.DBName);
                instance.EnsureUpToDateSchema();
            }
            return instance;
        }
    }
    static DBService instance; 

    private SQLiteConnection _connection;

    private List<Action<SQLiteConnection>> migrations = new List<Action<SQLiteConnection>>
    {
        // Version 1
        db =>
        {
            db.CreateTable<Language>();
            db.CreateTable<Prompt>();

            // to use foreign keys with SQLite, it needs to be done explicitly
            // PromptLoc
            db.Execute(@"
                CREATE TABLE IF NOT EXISTS PromptLoc (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PromptId INTEGER,
                    LangId INTEGER,
                    Text TEXT,
                    Available INTEGER,
                    FOREIGN KEY(PromptId) REFERENCES Prompt(Id)
                    FOREIGN KEY(LangId) REFERENCES Language(Id)
                );");
        },

        //// Version 2
        //db =>
        //{
        //    db.CreateTable<Logs>();
        //},

        //// Version 3
        //db =>
        //{
        //    // db.Execute("ALTER TABLE User ADD COLUMN NewField TEXT"); // example
        //    Debug.Log("Migration 3 complete.");
        //}
    };

    public IQueryable<Prompt> Prompts => _connection.Table<Prompt>().AsQueryable();
    
    #region ctor
    public DBService(string databaseName)
    {
        var dbPath = GetSystemSpecificPathToDBFile(databaseName);
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _connection.Execute("PRAGMA foreign_keys = ON;");
        Debug.Log("Final PATH: " + dbPath);
    }

    private string GetSystemSpecificPathToDBFile(string databaseName)
    {
#if UNITY_EDITOR
        return string.Format(@"Assets/StreamingAssets/{0}", databaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, databaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + databaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + databaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + databaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        return filepath;
#endif
    }
    #endregion ctor

    #region migrating

    public void EnsureUpToDateSchema()
    {
        _connection.CreateTable<Meta>();
        int version = GetSchemaVersion();

        if (version < migrations.Count && version > 0)
        {
            BackUpDB(Constants.DBName);
        }

        for (int i = version; i < migrations.Count; i++)
        {
            try
            {
                Debug.Log($"starting migration to DB version {i + 1}");
                migrations[i](_connection);
                SetSchemaVersion(i + 1); // schema version is 1-based
                Debug.Log($"Migrated to version {i + 1}");
            }
            catch (Exception e)
            {
                Debug.Log($"migrating DB to version {i + 1} failed");
                Debug.Log(e.Message);
                break;
            }
        }
    }

    private int GetSchemaVersion()
    {
        var entry = _connection.Find<Meta>(Meta.SchemaVersionKey);
        return entry != null && int.TryParse(entry.Value, out var version)
            ? version 
            : 0;
    }

    private void SetSchemaVersion(int version)
    {
        var meta = new Meta (){
            Key = Meta.SchemaVersionKey,
            Value = version.ToString()
        };
        _connection.InsertOrReplace(meta);
    }

    #endregion migrating

    #region backups

    public string ChooseBackUpDirectoryForDB()
    {
        var defaultPathObj = _connection.Find<Meta>(Meta.DBBackUpDirKey);
        var defaultPath = Directory.Exists(defaultPathObj?.Value) 
            ? defaultPathObj?.Value 
            : "";

        string path = EditorUtility.OpenFolderPanel("Select directory for backing up DB", defaultPath , "");
        // currently if the dialogue is closed without selecting, I back up the DB anyway just in the default location
        if (!string.IsNullOrWhiteSpace(path) && path != defaultPath)
        {
            if (defaultPathObj == null)
            {
                defaultPathObj = new Meta() { Key = Meta.DBBackUpDirKey, Value = path };
                _connection.Insert(defaultPathObj);
            }
            else
            {
                defaultPathObj.Value = path;
                _connection.Update(defaultPathObj);
            }
        }

        return string.IsNullOrWhiteSpace(path)
            ? Constants.DBBackUpDir 
            : path;
    }

    public bool BackUpDB(string databaseName)
    {
        return BackUpDB(databaseName, ChooseBackUpDirectoryForDB());
    }

    public bool BackUpDB(string databaseName, string destinationDir)
    {
        var nameSubstring = databaseName.Substring(0, databaseName.LastIndexOf('.'));
        var newName = $"{nameSubstring}{DateTime.Now.ToString("yyyyMMdd-HHmm")}.db";
        return BackUpDB(databaseName, destinationDir, newName);
    }

    private bool BackUpDB(string databaseName, string destinationDir, string newName)
    {
        try
        {
            var dbPath = GetSystemSpecificPathToDBFile(databaseName);
            if (!File.Exists(dbPath))
            {
                Debug.Log("no DB to back up");
                return false;
            }

            Utils.EnsureDirectoryExists(destinationDir);
            var destinationPath = Path.Combine(destinationDir, newName);
            File.Copy(dbPath, destinationPath, true);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }

    #endregion backups
}
