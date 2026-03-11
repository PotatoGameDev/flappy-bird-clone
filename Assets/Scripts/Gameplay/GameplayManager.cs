using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public TextMeshProUGUI populationLabel;
    public TextMeshProUGUI rpmLabel;
    public TextMeshProUGUI gateCounterLabel;
    public TextMeshProUGUI energyLabel;

    public int gateCount = 0;

    [SerializeField] private GameObject fadingTextCasualtiesPrefab;
    [SerializeField] private GameObject fadingTextEnergyPrefab;
    [SerializeField] private GameObject fadingTextPopulationAddedPrefab;

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
                AddVanishingText(text, populationLabel.transform, fadingTextPopulationAddedPrefab);
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
                AddVanishingText(text, populationLabel.transform, fadingTextPopulationAddedPrefab);
            }
            yield return EVERY_SECOND;
        }
    }

    private void UpdateLabels()
    {
        populationLabel.text = currentPopulation.ToString();
        rpmLabel.text = "RPM: " + GameManager.Instance.Player.GetRpm();
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

        currentPopulation -= peopleDied;

        AddPopulationLossText(peopleDied);

        UpdateLabels();

        if (currentPopulation == 0)
        {
            Death();
        }
    }

    private void AddPopulationLossText(long peopleDied)
    {
        float diedPercent = Mathf.Floor(peopleDied / (float)(peopleDied + currentPopulation) * 100f);

        string text = casualtiesTexts[Random.Range(0, casualtiesTexts.Length - 1)];

        text += " (" + diedPercent.ToString("0") + "%)"; // "0" removes the decimal part

        text = string.Format(text, peopleDied);

        AddVanishingText(text, populationLabel.transform, fadingTextCasualtiesPrefab);
    }

    private void AddVanishingText(string text, Transform parent, GameObject prefab)
    {
        GameObject fadingText = Instantiate
            (
                prefab,
                parent.position,
                Quaternion.identity,
                parent.parent
            );

        FadingTextController ftc = fadingText.GetComponent<FadingTextController>();

        ftc.Init(text);
    }

    public void Death()
    {
        GameManager.Instance.Save();
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
        GameObject fadingText = Instantiate(fadingTextEnergyPrefab, gateCounterLabel.transform.position, Quaternion.identity, gateCounterLabel.transform.parent);

        FadingTextController ftc = fadingText.GetComponent<FadingTextController>();
        ftc.Init("+" + energyAdded + "GW");
    }


}


