using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

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

    [SerializeField] private Slider spinDoctorLevelSlider;
    [SerializeField] private GameObject spinDoctorLevelSliderFill;
    public float SpinDoctorUsagePerSecond { get; private set; }
    private static readonly int SPIN_DOCTOR_RPM_PER_LEVEL = 100;
    private float spinDoctorLeft;


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

        // AndroidSlavery
        int androidSlaveryLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.AndroidSlavery).Level;
        if (androidSlaveryLevel > 0)
        {
            StartCoroutine(DoAndroidSlavery(androidSlaveryLevel));
        }

        // VyagraEnergizingTherapy
        int vyagraEnergizingTherapy = UpgradesManager.Instance.GetUpgrade(
                UpgradeId.VyagraEnergizingTherapy
                ).Level;
        if (vyagraEnergizingTherapy > 0)
        {
            StartCoroutine(DoVyagraEnergizingTherapy(vyagraEnergizingTherapy));
        }

        // SpinDoctor
        int spinDoctorLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.SpinDoctor).Level;

        if (spinDoctorLevel == 0)
        {
            spinDoctorLevelSliderFill.SetActive(false);
        }
        else
        {
            spinDoctorLeft = spinDoctorLevel * SPIN_DOCTOR_RPM_PER_LEVEL;
            spinDoctorLevelSlider.minValue = 0f;
            spinDoctorLevelSlider.maxValue = spinDoctorLeft;
            spinDoctorLevelSlider.value = spinDoctorLeft;
        }
    }

    IEnumerator DoAndroidSlavery(int level)
    {
        while (true)
        {
            if (Player.Dead()) break;

            long increase = level * 100;
            if (increase > 0)
            {
                currentPopulation += increase;
                string text = "+" + increase;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                UpdateLabels();
            }
            yield return EVERY_SECOND;
        }
    }

    IEnumerator DoVyagraEnergizingTherapy(int level)
    {
        float percent = level / 100f;
        while (true)
        {
            if (Player.Dead()) break;

            long increase = (long)(currentPopulation * percent);
            if (increase > 0)
            {
                currentPopulation += increase;
                string text = "+" + increase;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                UpdateLabels();
            }

            yield return EVERY_SECOND;
        }
    }

    void Update()
    {
        if (Player.Dead()) return;

        float rpm = Player.GetRpm();
        float rpmAbs = Mathf.Abs(rpm);
        if (rpmAbs > 0f)
        {
            // Here we calculate the max RPM we can damp with Spin Doctor, and if it's not 0 then we run the particles
            // We also decrease the available Spin Doctor level.
            float corrected = Mathf.Lerp(rpm, 0f, 0.5f * Time.deltaTime); // We correct half of the total rotation per second
            float rpmDamped = rpmAbs - Mathf.Abs(corrected);

            rpmDamped = Mathf.Clamp(rpmDamped, 0f, spinDoctorLeft);

            spinDoctorLeft -= rpmDamped;
            spinDoctorLevelSlider.value = spinDoctorLeft;
            if (spinDoctorLeft <= 0f)
            {
                spinDoctorLevelSliderFill.SetActive(false);
            }

            Player.AddRpm(rpmDamped * Mathf.Sign(rpm) * -1f);
            UpdateRpmLabel();

            SpinDoctorUsagePerSecond = rpmDamped / Time.deltaTime;
        }
    }

    private void UpdateLabels()
    {
        populationLabel.text = "POP: " + currentPopulation.ToString();
        gateCounterLabel.text = gateCount.ToString();
        energyLabel.text = GameManager.Instance.CollectedEnergy + "GW";
        UpdateRpmLabel();
    }

    private void UpdateRpmLabel()
    {
        rpmLabel.text = "RPM: " + (int)Mathf.Abs(Player.GetRpm());
    }

    public long TakeHit(float force)
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

        return peopleDied;
    }

    private void KillPopulation(int dead)
    {
        if (currentPopulation <= dead)
        {
            currentPopulation = 0;
        }
        else
        {
            currentPopulation -= dead;
        }

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
        int collectedEnergy = EnergyPerGate();

        GameManager.Instance.CollectedEnergy += collectedEnergy;
        AddEnergyCollectedText(collectedEnergy);
        UpdateLabels();
    }

    public int EnergyPerGate()
    {
        return UpgradesManager.Instance.GetUpgrade(UpgradeId.ORing).Level + 1;
    }

    private void AddEnergyCollectedText(int energyAdded)
    {
        string text = "+" + energyAdded + "GW";

        energyMessagesManager.Spawn(text, fadingMessageEnergyColor);
    }


}


