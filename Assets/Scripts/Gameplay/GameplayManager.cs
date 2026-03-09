using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    public TextMeshProUGUI populationLabel;
    public TextMeshProUGUI rpmLabel;
    public TextMeshProUGUI gateCounterLabel;

    public int gateCount = 0;

    [SerializeField] private GameObject fadingTextPrefab;
    [SerializeField] private GameObject fadingTextEnergyPrefab;

    private long currentPopulation = 0;

    private string[] casualtiesTexts = {
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

    void OnEnable()
    {
    }

    void Start()
    {

        // TODO: might be replaying previous level, have to pass the current civ type from menu to here
        currentPopulation = GameManager.Instance.GetBasePopulation();

        UpdateLabels();
    }

    private void UpdateLabels()
    {
        populationLabel.text = currentPopulation.ToString();
        rpmLabel.text = "RPM: " + GameManager.Instance.Player.GetRpm();
        gateCounterLabel.text = gateCount.ToString();
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

        GameObject fadingText = Instantiate(fadingTextPrefab, populationLabel.transform.position, Quaternion.identity, populationLabel.transform.parent);

        FadingTextController ftc = fadingText.GetComponent<FadingTextController>();

        string text = casualtiesTexts[Random.Range(0, casualtiesTexts.Length - 1)];

        text += " (" + diedPercent.ToString("0") + "%)"; // Removes the decimal part

        ftc.Init(string.Format(text, peopleDied));
    }

    public void Death()
    {
        GameManager.Instance.Save();
        SceneManager.LoadScene("NewMenu");
    }

    public void CollectEnergy()
    {
        gateCount++;

        int collectedEnergy = UpgradesManager.Instance.GetUpgrade(UpgradeId.ORing).Level + 1; // Initial level is 0, so we need to add 1.

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


