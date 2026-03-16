using UnityEngine;

public static class SaveSystem
{
    private const string SaveKey = "SaveData";

    public static void Save(GameState gameState)
    {
        string json = JsonUtility.ToJson(gameState);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static GameState Load()
    {
        if (!StateExists())
            return new GameState();

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<GameState>(json);
    }

    public static bool StateExists()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
