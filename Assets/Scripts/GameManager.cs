using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public PlanetController Player { get; set; }

    public GameState State = new();

    public long[] PopulationBasis = {
        1000000, 9000000000, 1000000000000,
    };

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


    public long GetPopulationForLevel(int level)
    {
        int currentCivType = State.CivTypePassed;
        long totalPopChange = 0;

        for (int i = 0; i < level; i++)
        {
            totalPopChange += State.PopulationChange[i];
        }

        return PopulationBasis[currentCivType] + totalPopChange;
    }
}

[System.Serializable]
public class GameState
{
    public long[] PopulationChange;
    public int CivTypePassed;
}
