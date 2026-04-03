using UnityEngine;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-100)]
public class UpgradesManager : MonoBehaviour
{
    private static readonly long SHIELD_AMOUNT_PER_LEVEL = 1000000;
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
        // TODO: Maybe a scriptable object?
        switch (ident)
        {
            case UpgradeId.ORing:
                int currentGateEnergyParticles = GetORingEnergyPerLevel();
                int nextGateEnergyParticles = GetORingEnergyPerLevel(+1);

                return $@"A ring of collectors that catch the energy exhausted from the space pipes.
The higher the level, the more energy particles are observed to eject from the pipes.
The higher the gate number, the bigger the chance for a high level energy particle, 
giving more energy.

Current gate energy particles: {currentGateEnergyParticles}
Next gate energy particles: {nextGateEnergyParticles}";
            case UpgradeId.AccretioSuction:
                int currentRadiationParticlesPerSecond = GetEnergyRadiationPerSecond(UpgradeId.AccretioSuction);
                int nextRadiationParticlesPerSecond = GetEnergyRadiationPerSecond(UpgradeId.AccretioSuction, +1);
                return $@"Detects the energy particles that are radiated from that big, gaping black hole at the top.
The higher the level, the more energy particles are observed to eject.
The higher the gate number, the bigger the chance for a high level energy particle, 
giving more energy.

Current black hole energy particles: {currentRadiationParticlesPerSecond}
Next black hole energy particles: {nextRadiationParticlesPerSecond}";

            case UpgradeId.StarLifting:
                int currentRadiationParticlesPerSecondStar = GetEnergyRadiationPerSecond(UpgradeId.StarLifting);
                int nextRadiationParticlesPerSecondStar = GetEnergyRadiationPerSecond(UpgradeId.StarLifting, +1);
                return $@"Detects the energy particles that are radiated from our SunStar at the bottom.
The higher level, the more energy particles are observed to eject.
The higher the gate number, the bigger the chance for a high level energy particle, 
giving more energy.

Current SunStar energy particles: {currentRadiationParticlesPerSecondStar}
Next SunStar energy particles: {nextRadiationParticlesPerSecondStar}";
            case UpgradeId.AndroidSlavery:
                long currentPopulationNumberPerSecond = GetPopulationNumberPerSecond(UpgradeId.AndroidSlavery);
                long nextPopulationNumberPerSecond = GetPopulationNumberPerSecond(UpgradeId.AndroidSlavery, +1);
                return $@"Allows building artificial antropomorphic droids, that are totally not humans. They have no feelings.
They don't mind being used as a free workforce, the scientists think. No matter what the androids keep saying.
This produces an amount of additional population every second.
One would argue that this is totally not even immoral.

Current additional population per second: {currentPopulationNumberPerSecond}
Next additional population per second: {nextPopulationNumberPerSecond}";
            case UpgradeId.VyagraEnergizingTherapy:
                float currentPopulationPercentPerSecond = GetPopulationPercentPerSecond(UpgradeId.VyagraEnergizingTherapy);
                float nextPopulationPercentPerSecond = GetPopulationPercentPerSecond(UpgradeId.VyagraEnergizingTherapy, +1);
                return $@"Spend energy for extraction of a blue space spice from the planet V-yag'ra.
It allows the population to be more... energetic. Somehow they can even reproduce with the androids, go figure...
It generates additional percentage of population.
If you know what ""Compound Interest"" is, or watched Spiffing Brit, then you understand what this means...

Current additional percentage of population per second: {currentPopulationPercentPerSecond}
Next additional percentage of population per second: {nextPopulationPercentPerSecond}";
            case UpgradeId.InstantPopulation:
                long currentBasePopulation = GameManager.Instance.GetBasePopulation();
                long instantBasePopulationIncrease = GetBasePopulationNumberInstant(+1);
                long nextBasePopulation = currentBasePopulation + instantBasePopulationIncrease;
                return $@"This instantly and permanently increases the base population by a given number.
Just add water and stir!

Current base population: {currentBasePopulation}
Next base population: {nextBasePopulation} (+{instantBasePopulationIncrease})";
            case UpgradeId.EnergyShield:
                long currentEnergyShieldAbsorb = GetEnergyShieldMax();
                long nextEnergyShieldAbsorb = GetEnergyShieldMax(+1);
                return $@"Absorbs some damage from collisions. 
Also absorbs black hole and sun damage.
Does not protect from spin damage tho...

Current max population damage absorbed: {currentEnergyShieldAbsorb}
Next max population damage absorbed: {nextEnergyShieldAbsorb}";

            case UpgradeId.SpinDoctor:
                long currentSpinDoctorRpm = GetSpinDoctorMaxRpmPerSecond();
                long nextSpinDoctorRpm = GetSpinDoctorMaxRpmPerSecond(+1);
                return $@"Thrusters distributed around the planet, that reduce planetary spin up to 0RPM.
The thrusters do kill a marginal number of people and endangered species,
but the rest of us can enjoy not being ejected into the void of space.

Current max RPM reduced: {currentSpinDoctorRpm}
Next max RPM reduced: {currentSpinDoctorRpm}";
            case UpgradeId.ToorboBoost:
                int currentToorboBoostSeconds = GetToorboBoostSecondsForLevel();
                int nextToorboBoostSeconds = GetToorboBoostSecondsForLevel(+1);
                return $@"Thrusters distributed at the poles that allow to speed up if the planet lags too much.
Allows to avoid a premature ejection from the space pipes tunnel.
Each level adds additional seconds of speed up.
The planet will speed up only until it reaches the initial position.

Current max boost usage time: {currentToorboBoostSeconds}
Next max boost usage time: {nextToorboBoostSeconds}";

            default:
                return "TODO";

        }
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
            float vyagraEnergizingTherapyPercent = level / 100f;
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

    public long GetEnergyShieldMax(int level = 0)
    {
        level += GetUpgrade(UpgradeId.EnergyShield).Level;

        return level * SHIELD_AMOUNT_PER_LEVEL;
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
