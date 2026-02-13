using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(GameState gameState)
    {
        string json = JsonUtility.ToJson(gameState);
        File.WriteAllText(SavePath, json);
    }

    public static GameState Load()
    {
        if (!StateExists())
        {
            return new GameState
            {
                CivType = 1,
                Population = 1000000
            };
        }
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<GameState>(json);
    }

    public static bool StateExists()
    {
        return File.Exists(SavePath);
    }

    public static void Reset()
    {
        File.Delete(SavePath);
    }
}
