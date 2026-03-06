using UnityEngine;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-100)]
public class UpgradesManager : MonoBehaviour
{
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
    }

    public event Action<Upgrade> OnUpgrade;

    private static readonly Dictionary<UpgradeId, Upgrade> upgrades = new()
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
                10,
                2
                ),
        [UpgradeId.VyagraEnergizingTherapy] = new(
                UpgradeId.VyagraEnergizingTherapy,
                "V'yag-ra Energizing Therapy",
                10,
                2
                ),
        [UpgradeId.PopulationFactory] = new(
                UpgradeId.PopulationFactory,
                "Population Factory",
                10
                ),
        [UpgradeId.ToorboBoost] = new(
                UpgradeId.ToorboBoost,
                "Toorbo Boost",
                10,
                2
                ),
        [UpgradeId.EnergyShield] = new(
                UpgradeId.EnergyShield,
                "Energy Shield",
                10,
                2
                ),
        [UpgradeId.SpinDoctor] = new(
                UpgradeId.SpinDoctor,
                "Spin Doctor",
                10,
                2
                ),
    };

    public Upgrade GetUpgrade(UpgradeId ident)
    {
        Upgrade upgrade = upgrades[ident];
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
    }

    private void DoUpgradeSpecialLogic(Upgrade upgrade)
    {
        switch (upgrade.Ident)
        {
            case UpgradeId.PopulationFactory:
                {
                    GameManager.Instance.AddBasePopulation(upgrade.Level * 1000);
                    break;
                }
        }
    }
}

public enum UpgradeId
{
    ORing,
    AccretioSuction,
    StarLifting,
    AndroidSlavery,
    VyagraEnergizingTherapy,
    PopulationFactory,
    ToorboBoost,
    EnergyShield,
    SpinDoctor
}

public class Upgrade
{
    public UpgradeId Ident { get; }
    public string Name { get; }
    public string Description { get; }
    public int EnergyCost { get; }
    public int MaxLevel { get; }
    public int Level { get; private set; }

    public Upgrade(
            UpgradeId ident,
            string name,
            int energyCost,
            int maxLevel = 1000,
            string description = "TODO"
            )
    {
        Ident = ident;
        Name = name;
        Description = description;
        EnergyCost = energyCost;
        Level = 0;
        MaxLevel = maxLevel;
    }


    public void Buy()
    {
        if (Level >= MaxLevel) throw new InvalidOperationException("Level already maxed, this should be blocked on UI");
        Level++;
    }

    public int GetTotalPrice()
    {
        // TODO More granular logic for pricing
        return (Level + 1) * EnergyCost;
    }

}
