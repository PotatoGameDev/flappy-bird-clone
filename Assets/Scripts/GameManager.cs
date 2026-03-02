using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public PlanetController Player { get; set; }

    public GameState State = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        State = SaveSystem.Load();
    }

    public void Save()
    {
        SaveSystem.Save(State);
    }
}

[System.Serializable]
public class GameState
{
    public long[] SurvivingPopulation;
    public int CivTypePassed;

    public long GetStartingPopulation(int level)
    {
        // if this is the first level, we return the full starting population
        if (level == 0) return 9000000;

        // else, we return the surviving population for the previous level
        return SurvivingPopulation[level - 1];
    }

    public long GetSurvivingPopulation(int level)
        => SurvivingPopulation[level];
}
