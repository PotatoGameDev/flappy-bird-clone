using UnityEngine;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-100)]
public class UpgradesManager : MonoBehaviour
{
    private static readonly long SHIELD_AMOUNT_PER_LEVEL = 10_000;
    internal static readonly long SHIELD_AMOUNT_PER_ENERGY = 100;
    private static readonly float SHIELD_SIZE_PER_LEVEL = 0.01f;
    private static readonly int SPIN_DOCTOR_RPM_PER_LEVEL = 100;
    private static readonly int TOORBO_BOOST_SECONDS_PER_LEVEL = 5;

    public static UpgradesManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Not singleton related:

        Load(GameManager.Instance.GetUpgradesState());
    }

    public void Load(List<UpgradeState> upgrades)
    {
        ClearUpgrades();

        foreach (UpgradeState us in upgrades)
        {
            State[us.Ident].Load(us.Level);
        }
    }

    public void ClearUpgrades()
    {
        foreach (UpgradeId upgradeId in Enum.GetValues(typeof(UpgradeId)))
        {
            State[upgradeId].Load(0);
        }
    }

    public event Action<Upgrade> OnUpgrade;

    public readonly Dictionary<UpgradeId, Upgrade> State = new()
    {
        [UpgradeId.ORing] = new(
                UpgradeId.ORing,
                "O-Ring",
                10
                ),
        [UpgradeId.AccretioSuction] = new(
                UpgradeId.AccretioSuction,
                "AccretioSuction",
                10
                ),
        [UpgradeId.StarLifting] = new(
                UpgradeId.StarLifting,
                "Star Lifting",
                10
                ),
        [UpgradeId.AndroidSlavery] = new(
                UpgradeId.AndroidSlavery, "Android Slavery",
                10
                ),
        [UpgradeId.VyagraEnergizingTherapy] = new(
                UpgradeId.VyagraEnergizingTherapy,
                "V'yag-ra Energizing Therapy",
                10
                ),
        [UpgradeId.InstantPopulation] = new(
                UpgradeId.InstantPopulation,
                "Instant Population",
                10
                ),
        [UpgradeId.ToorboBoost] = new(
                UpgradeId.ToorboBoost,
                "Toorbo Boost",
                10
                ),
        [UpgradeId.EnergyShield] = new(
                UpgradeId.EnergyShield,
                "Energy Shield",
                10
                ),
        [UpgradeId.SpinDoctor] = new(
                UpgradeId.SpinDoctor,
                "Spin Doctor",
                10
                ),
    };

    public Upgrade GetUpgrade(UpgradeId ident)
    {
        Upgrade upgrade = State[ident];
        Debug.AssertFormat(upgrade != null, "Ident {0} does not exist in upgrades, a typo?", ident);
        return upgrade;
    }

    public void Buy(UpgradeId ident)
    {
        Upgrade u = GetUpgrade(ident);

        int price = u.GetTotalPrice();

        if (GameManager.Instance.CollectedEnergy < price)
            throw new InvalidOperationException("Not enough energy, should be blocked in UI");

        GameManager.Instance.CollectedEnergy -= price;

        u.Buy();

        DoUpgradeSpecialLogic(u);

        OnUpgrade?.Invoke(u);

        GameManager.Instance.Save();
    }

    private void DoUpgradeSpecialLogic(Upgrade upgrade)
    {
        switch (upgrade.Ident)
        {
            case UpgradeId.InstantPopulation:
                {
                    GameManager.Instance.AddBasePopulation(GetBasePopulationNumberInstant());
                    break;
                }
        }
    }

    public string GetUpgradeDescription(UpgradeId ident)
    {
        string locKey = Upgrade.GetDescriptionLocKey(ident);

        object currentValue;
        object nextValue;

        switch (ident)
        {
            case UpgradeId.ORing:
                currentValue = GetORingEnergyPerLevel();
                nextValue = GetORingEnergyPerLevel(+1);
                break;
            case UpgradeId.AccretioSuction:
                currentValue = GetEnergyRadiationPerSecond(UpgradeId.AccretioSuction);
                nextValue = GetEnergyRadiationPerSecond(UpgradeId.AccretioSuction, +1);
                break;
            case UpgradeId.StarLifting:
                currentValue = GetEnergyRadiationPerSecond(UpgradeId.StarLifting);
                nextValue = GetEnergyRadiationPerSecond(UpgradeId.StarLifting, +1);
                break;
            case UpgradeId.AndroidSlavery:
                currentValue = GetPopulationNumberPerSecond(UpgradeId.AndroidSlavery);
                nextValue = GetPopulationNumberPerSecond(UpgradeId.AndroidSlavery, +1);
                break;
            case UpgradeId.VyagraEnergizingTherapy:
                currentValue = GetPopulationPercentPerSecond(UpgradeId.VyagraEnergizingTherapy);
                nextValue = GetPopulationPercentPerSecond(UpgradeId.VyagraEnergizingTherapy, +1);
                break;
            case UpgradeId.InstantPopulation:
                long currentBasePopulation = GameManager.Instance.GetBasePopulation();
                long currentValueLong = GetBasePopulationNumberInstant(+1);
                currentValue = currentValueLong;
                nextValue = currentBasePopulation + currentValueLong;
                break;
            case UpgradeId.EnergyShield:
                currentValue = GetEnergyShieldPowerMax();
                nextValue = GetEnergyShieldPowerMax(+1);
                float currentSize = GetEnergyShieldSizeMax();
                float nextSize = GetEnergyShieldSizeMax(+1);
                // TODO Maybe a separate upgrade for that?
                return Loc.Get(
                        locKey,
                        currentValue,
                        nextValue,
                        currentSize,
                        nextSize);
            case UpgradeId.SpinDoctor:
                currentValue = GetSpinDoctorMaxRpmPerSecond();
                nextValue = GetSpinDoctorMaxRpmPerSecond(+1);
                break;
            case UpgradeId.ToorboBoost:
                currentValue = GetToorboBoostSecondsForLevel();
                nextValue = GetToorboBoostSecondsForLevel(+1);
                break;

            default:
                return "TODO";
        }

        return Loc.Get(
                locKey,
                currentValue,
                nextValue
            );
    }

    /// Upgrade params:
    /// Those methods return current increase (for current level) if no "level" provided, and current + level if level provided.
    public int GetORingEnergyPerLevel(int level = 0)
    {
        level += GetUpgrade(UpgradeId.ORing).Level;
        return level + 1;
    }

    public int GetEnergyRadiationPerSecond(UpgradeId ident, int level = 0)
    {
        Debug.Assert(
                ident == UpgradeId.StarLifting || ident == UpgradeId.AccretioSuction,
                "Energy radiation wrong UpgradeId " + ident
                );
        level += GetUpgrade(ident).Level;
        return level;
    }

    public float GetPopulationPercentPerSecond(UpgradeId ident, int level = 0)
    {
        Debug.Assert(
                ident == UpgradeId.VyagraEnergizingTherapy,
                "Population increase wrong UpgradeId " + ident
                );
        level += GetUpgrade(ident).Level;

        if (ident == UpgradeId.VyagraEnergizingTherapy)
        {
            // This gives values like:
            // 1 - 0.6
            // ...
            // 10 - 2.08
            // ...
            // 30 - 3.01
            // ...
            // 100 - 4.0
            float vyagraEnergizingTherapyPercent = Mathf.Log10(level + 1.0f) * 0.02f;


            return vyagraEnergizingTherapyPercent;
        }
        return 0f;
    }

    public long GetPopulationNumberPerSecond(UpgradeId ident, int level = 0)
    {
        Debug.Assert(
                ident == UpgradeId.AndroidSlavery,
                "Population increase wrong UpgradeId " + ident
                );
        level += GetUpgrade(ident).Level;

        if (ident == UpgradeId.AndroidSlavery)
        {
            return level * 100L;
        }
        return 0L;
    }

    public long GetBasePopulationNumberInstant(int level = 0)
    {
        level += GetUpgrade(UpgradeId.InstantPopulation).Level;

        return level * 1000L;
    }

    public long GetEnergyShieldPowerMax(int level = 0)
    {
        level += GetUpgrade(UpgradeId.EnergyShield).Level;

        return level * SHIELD_AMOUNT_PER_LEVEL;
    }

    public float GetEnergyShieldSizeMax(int level = 0)
    {
        level += GetUpgrade(UpgradeId.EnergyShield).Level;

        return 1 + level * SHIELD_SIZE_PER_LEVEL;
    }

    public int GetSpinDoctorMaxRpmPerSecond(int level = 0)
    {
        level += GetUpgrade(UpgradeId.SpinDoctor).Level;

        return level * SPIN_DOCTOR_RPM_PER_LEVEL;
    }

    public int GetToorboBoostSecondsForLevel(int level = 0)
    {
        level += GetUpgrade(UpgradeId.ToorboBoost).Level;

        return level * TOORBO_BOOST_SECONDS_PER_LEVEL;
    }
}

public enum UpgradeId
{
    ORing,
    AccretioSuction,
    StarLifting,
    AndroidSlavery,
    VyagraEnergizingTherapy,
    InstantPopulation,
    ToorboBoost,
    EnergyShield,
    SpinDoctor
}

public class Upgrade
{
    public UpgradeId Ident { get; }
    public string Name { get; }
    public int EnergyCost { get; }
    public int MaxLevel { get; }
    public int Level { get; private set; }

    public string LocKey
    {
        get
        {
            return GetLocKey(Ident);
        }
    }

    public static string GetLocKey(UpgradeId ident)
    {
        return "upgrade_" + ident.ToString().ToLower();
    }

    public static string GetDescriptionLocKey(UpgradeId ident)
    {
        return GetLocKey(ident) + "_description";
    }

    public Upgrade(
            UpgradeId ident,
            string name,
            int energyCost,
            int maxLevel = 1000
            )
    {
        Ident = ident;
        Name = name;
        EnergyCost = energyCost;
        Level = 0;
        MaxLevel = maxLevel;
    }


    public void Buy()
    {
        if (Level >= MaxLevel) throw new InvalidOperationException("Level already maxed, this should be blocked on UI");
        Level++;
    }

    public void Load(int level)
    {
        Level = level;
    }

    public int GetTotalPrice()
    {
        // TODO More granular logic for pricing
        return (Level + 1) * EnergyCost;
    }

}
