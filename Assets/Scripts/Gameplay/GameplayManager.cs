using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

[DefaultExecutionOrder(-500)]
public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public PlanetController Player { get; set; }

    [SerializeField] private TextMeshProUGUI populationLabel;
    [SerializeField] private TextMeshProUGUI rpmLabel;
    [SerializeField] private TextMeshProUGUI gateCounterLabel;
    [SerializeField] private TextMeshProUGUI energyLabel;

    private int gateCount = 0;

    [SerializeField] private Color fadingMessageCasualtiesColor;
    [SerializeField] private Color fadingMessageEnergyColor;
    [SerializeField] private Color fadingMessagePopulationAddedColor;

    [SerializeField] private FadingMessagesManager populationMessagesManager;
    [SerializeField] private FadingMessagesManager energyMessagesManager;

    private long currentPopulation = 0;

    private readonly static WaitForSeconds EVERY_SECOND = new(1f);


    private readonly string[] casualtiesTexts = {
        "{0} died",
        "{0} killed",
        "{0} squashed",
        "{0} evaporated",
        "{0} lost",
        "{0} are no more",
        "{0} are now ex-people",
        "{0} are poorly",
        "{0} need some milk",
        "{0} have a bad feeling about this",
        "{0} did redeem, ma'am",
        "{0} have no fun",
        "{0} perished",
        "{0} don't get no respect",
    };

    private readonly string[] rotationCasualtiesTexts = {
        "{0} suffocated",
        "{0} boiled",
        "{0} no one could hear scream",
        "{0} are lost in space",
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {

        // TODO: might be replaying previous level, have to pass the current civ type from menu to here
        currentPopulation = GameManager.Instance.GetBasePopulation();

        UpdateLabels();

        int androidSlaveryLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.AndroidSlavery).Level;
        if (androidSlaveryLevel > 0)
        {
            StartCoroutine(DoAndroidSlavery(androidSlaveryLevel));
        }

        int vyagraEnergizingTherapy = UpgradesManager.Instance.GetUpgrade(
                UpgradeId.VyagraEnergizingTherapy
                ).Level;
        if (vyagraEnergizingTherapy > 0)
        {
            StartCoroutine(DoVyagraEnergizingTherapy(vyagraEnergizingTherapy));
        }
    }

    IEnumerator DoAndroidSlavery(int level)
    {
        while (true)
        {
            long increase = level * 100;
            if (increase > 0)
            {

                currentPopulation += increase;
                string text = "+" + increase;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
            }
            yield return EVERY_SECOND;
        }
    }

    IEnumerator DoVyagraEnergizingTherapy(int level)
    {
        float percent = level / 100f;
        while (true)
        {
            long increase = (long)(currentPopulation * percent);
            if (increase > 0)
            {
                currentPopulation += increase;
                string text = "+" + increase;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
            }
            yield return EVERY_SECOND;
        }
    }

    private void UpdateLabels()
    {
        populationLabel.text = "POP: " + currentPopulation.ToString();
        rpmLabel.text = "RPM: " + Player.GetRpm();
        gateCounterLabel.text = gateCount.ToString();
        energyLabel.text = GameManager.Instance.CollectedEnergy + "GW";
    }

    public void TakeHit(float force)
    {
        // Calculate casualties:
        float maxHitPercent = 100f;
        float maxHitForce = 20f;

        int minCasualties = 1000;

        float hitPercent = maxHitPercent * Mathf.Clamp01(force / maxHitForce);
        long peopleDied = (long)(currentPopulation * (hitPercent / 100f));

        if (peopleDied < minCasualties) peopleDied = minCasualties;
        if (peopleDied > currentPopulation) peopleDied = currentPopulation;

        AddPopulationLossText(peopleDied);

        KillPopulation((int)peopleDied);
    }

    private void KillPopulation(int dead)
    {

        currentPopulation -= dead;

        UpdateLabels();

        if (currentPopulation <= 0)
        {
            Player.Death();
        }
    }

    public void RotationalDamage(int dead)
    {
        AddPopulationLossTextShort(dead);

        KillPopulation(dead);
    }

    private void AddPopulationLossText(long peopleDied)
    {
        float diedPercent = Mathf.Floor(peopleDied / (float)(peopleDied + currentPopulation) * 100f);

        string text = casualtiesTexts[Random.Range(0, casualtiesTexts.Length - 1)];

        text += " (" + diedPercent.ToString("0") + "%)"; // "0" removes the decimal part

        text = string.Format(text, peopleDied);

        populationMessagesManager.Spawn(text, fadingMessageCasualtiesColor);
    }

    private void AddPopulationLossTextShort(long peopleDied)
    {
        string text = rotationCasualtiesTexts[Random.Range(0, rotationCasualtiesTexts.Length - 1)];

        text = string.Format(text, peopleDied);

        populationMessagesManager.Spawn(text, fadingMessageCasualtiesColor);

    }

    public void Death()
    {
        GameManager.Instance.Save();

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("NewMenu");
    }

    public void CollectEnergy()
    {
        gateCount++;

        // Initial level is 0, so we need to add 1.
        int collectedEnergy = UpgradesManager.Instance.GetUpgrade(UpgradeId.ORing).Level + 1;

        GameManager.Instance.CollectedEnergy += collectedEnergy;
        AddEnergyCollectedText(collectedEnergy);
        UpdateLabels();
    }

    private void AddEnergyCollectedText(int energyAdded)
    {
        string text = "+" + energyAdded + "GW";

        energyMessagesManager.Spawn(text, fadingMessageEnergyColor);
    }


}


