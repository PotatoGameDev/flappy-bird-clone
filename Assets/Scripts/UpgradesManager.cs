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

    private static readonly Dictionary<string, Upgrade> upgrades = new()
    {
        ["O-Ring"] = new Upgrade(
                "O-Ring",
                10
                ),
        ["Accretion Suction"] = new Upgrade(
                "",
                10
                ),
        ["Star Lifting"] = new Upgrade(
                "Star Lifting",
                10
                ),
        ["Android Slavery"] = new Upgrade(
                "Android Slavery",
                10,
                2
                ),
        ["V'yag-ra Energizing Therapy"] = new Upgrade(
                "V'yag-ra Energizing Therapy",
                10,
                2
                ),
        ["Toorbo"] = new Upgrade(
                "Toorbo",
                10,
                2
                ),
        ["Energy Shield"] = new Upgrade(
                "Energy Shield",
                10,
                2
                ),
        ["Spin Correction"] = new Upgrade(
                "Spin Correction",
                10,
                2
                ),
    };

    public Upgrade GetUpgrade(string ident)
    {
        Debug.Assert(ident != null, "Ident has to be passed.");
        Upgrade upgrade = upgrades[ident];
        Debug.AssertFormat(ident != null, "Ident {0} does not exist in upgrades, a typo?", ident);
        return upgrade;
    }

    public void Buy(string ident)
    {
        GameState state = GameManager.Instance.State;

        Upgrade u = GetUpgrade(ident);

        int price = u.GetTotalPrice();

        if (state.CollectedEnergy < price)
            throw new InvalidOperationException("Not enough energy, should be blocked in UI");

        state.CollectedEnergy -= price;

        u.Buy();

        OnUpgrade?.Invoke(u);
    }
}

public class Upgrade
{
    public string Name { get; }
    public string Description { get; }
    public int EnergyCost { get; }
    public int MaxLevel { get; }
    public int Level { get; private set; }

    public Upgrade(
            string name,
            int energyCost,
            int maxLevel = 1000,
            string description = "TODO"
            )
    {
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
