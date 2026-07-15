using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    //private readonly int[] energyToAdvance = { 100, 1000, 10000 };
    // For testing:
    private readonly int[] energyToAdvance = { 100, 100, 100 };


    // State things
    private GameState State;
    public event Action<GameState> OnGameStateChanged;

    public LevelSettings levelSettings = new()
    {
        levelType = LevelType.Normal
    };

    internal bool newLevelUnlocked;

    // 0 based! So there is level 0, 1, 2
    public int CurrentLevel
    {
        get
        {
            return Mathf.Clamp(State.CurrentLevel, 0, 2);
        }
        set
        {
            State.CurrentLevel = Mathf.Clamp(value, 0, 2);
            OnGameStateChanged?.Invoke(State);
            SaveSystem.Save(State);
        }
    }

    public long CollectedEnergy
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

    public int PlanetType
    {
        get
        {
            return State.PlanetType;
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

        Loc.LoadSavedLanguage();
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

    public long GetBasePopulation()
    {
        return State.BasePopulation;
    }

    public void AddBasePopulation(long toAdd)
    {
        State.BasePopulation += toAdd;

        OnGameStateChanged?.Invoke(State);
        SaveSystem.Save(State);
    }

    public List<UpgradeState> GetUpgradesState()
    {
        return State.Upgrades;
    }

    public int GetRequiredAdvanceEnergy()
    {
        return energyToAdvance[CurrentLevel];
    }

    public bool CanPlayLevel(int level = -1)
    {
        if (level < 0)
        {
            level = CurrentLevel;
        }
        return level <= State.CivTypePassed;
    }

    public bool CanAdvanceLevel()
    {
        return CurrentLevel == State.CivTypePassed
            && State.CollectedEnergy >= GetRequiredAdvanceEnergy();
    }

    public void UnlockNextPhase()
    {
        int advanceEnergy = GetRequiredAdvanceEnergy();
        Debug.Assert(CurrentLevel == State.CivTypePassed, "Cannot upgrade level if not in the max level currently");

        State.CollectedEnergy -= advanceEnergy;
        State.CivTypePassed++;
        CurrentLevel++;

        newLevelUnlocked = true;

        Save();

        OnGameStateChanged?.Invoke(State);
    }

    public void ResetGame()
    {
        State = new GameState
        {
            PlanetType = Random.Range(0, 10)
        };

        UpgradesManager.Instance.ClearUpgrades();
        Save();

        OnGameStateChanged?.Invoke(State);
    }
}

[Serializable]
public class GameState
{
    public long BasePopulation;
    public int CivTypePassed;
    public long CollectedEnergy;
    public int CurrentLevel; // 0 based! So there is level 0, 1, 2
    public List<UpgradeState> Upgrades;

    public int PlanetType = 0;

    public GameState()
    {
        BasePopulation = 1000000;
        CurrentLevel = 0;
        CivTypePassed = 0;
        CollectedEnergy = 0;
        Upgrades = new();
    }
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

public class LevelSettings
{
    public LevelType levelType = LevelType.Normal;
}

public enum LevelType
{
    Normal, BossFight
}
