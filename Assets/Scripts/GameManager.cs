using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
        // Here we dump the upgrades state in the form of a list of objects with upgrade ID and level.
        // This will be saved and loaded, way simpler then the whole UpgradeManager.State;
        Dictionary<UpgradeId, Upgrade> upgrades = UpgradesManager.Instance.State;

        State.Upgrades = upgrades.Values.Select(u => new UpgradeState(u.Ident, u.Level))
            .ToList();

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

    public List<UpgradeState> GetUpgradesState()
    {
        return State.Upgrades;
    }

    public void UnlockNextPhase()
    {
        Debug.Assert(State.CurrentLevel != State.CivTypePassed, "Cannot upgrade level if not in the max level currently");

        State.CivTypePassed++;

        Save();
    }
}

[Serializable]
public class GameState
{
    public long[] BasePopulation;
    public int CivTypePassed;
    public int CollectedEnergy;
    public int CurrentLevel;
    public List<UpgradeState> Upgrades;

    public GameState()
    {
        long[] basePopulation = { 9000000, 0, 0 };
        BasePopulation = basePopulation;
        CurrentLevel = 0;
        CivTypePassed = 0;
        CollectedEnergy = 0;
        Upgrades = new();
    }

    public long GetBasePopulation(int level)
        => BasePopulation[level];
}

[Serializable]
public class UpgradeState
{
    public UpgradeId Ident;
    public int Level;

    public UpgradeState(UpgradeId ident, int level)
    {
        Ident = ident;
        Level = level;
    }
}
