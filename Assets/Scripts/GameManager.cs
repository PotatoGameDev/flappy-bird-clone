using UnityEngine;
using System;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public PlanetController Player { get; set; }

    // State things
    private GameState State;
    public event Action<GameState> OnGameStateChanged;

    public int CurrentLevel
    {
        get
        {
            return State.CurrentLevel;
        }
        set
        {
            State.CurrentLevel = value;
            OnGameStateChanged?.Invoke(State);
            SaveSystem.Save(State);
        }
    }

    public int CollectedEnergy
    {
        get
        {
            return State.CollectedEnergy;
        }
        set
        {
            State.CollectedEnergy = value;
            OnGameStateChanged?.Invoke(State);
        }
    }

    public int CivTypePassed
    {
        get
        {
            return State.CivTypePassed;
        }
        set
        {
            State.CivTypePassed = value;
            OnGameStateChanged?.Invoke(State);
            SaveSystem.Save(State);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Other
        State = SaveSystem.Load();
    }

    public void Save()
    {
        SaveSystem.Save(State);
    }

    public long GetBasePopulation(int level)
    {
        return State.GetBasePopulation(level);
    }

    public long GetBasePopulation()
    {
        return State.GetBasePopulation(State.CurrentLevel);
    }

    public void AddBasePopulation(int toAdd)
    {
        State.BasePopulation[State.CurrentLevel] += toAdd;

        OnGameStateChanged?.Invoke(State);
        SaveSystem.Save(State);
    }
}

[Serializable]
public class GameState
{
    public long[] BasePopulation;
    public int CivTypePassed;
    public int CollectedEnergy;
    public int CurrentLevel;

    public GameState()
    {
        long[] basePopulation = { 9000000, 0, 0 };
        BasePopulation = basePopulation;
        CurrentLevel = 0;
        CivTypePassed = 0;
        CollectedEnergy = 0;
    }

    public long GetBasePopulation(int level)
        => BasePopulation[level];
}
