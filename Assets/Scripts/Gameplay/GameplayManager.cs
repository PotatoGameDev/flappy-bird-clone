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

    public LevelType LevelType { get; set; }

    private static readonly WaitForSeconds WAIT_2_SECONDS = new(2f);

    [SerializeField] private TextMeshProUGUI populationLabel;
    [SerializeField] private TextMeshProUGUI rpmLabel;
    [SerializeField] private TextMeshProUGUI gateCounterLabel;
    [SerializeField] private TextMeshProUGUI energyLabel;

    public int GateCount { get; set; } = 0;

    [SerializeField] private Color fadingMessageCasualtiesColor;
    [SerializeField] private Color fadingMessageEnergyColor;
    [SerializeField] private Color fadingMessagePopulationAddedColor;

    [SerializeField] private FadingMessagesManager populationMessagesManager;
    [SerializeField] private FadingMessagesManager energyMessagesManager;
    [SerializeField] private FadingMessagesManager rpmMessagesManager;

    private long currentPopulation = 0;

    private readonly static WaitForSeconds EVERY_SECOND = new(1f);

    [SerializeField] private Slider spinDoctorLevelSlider;
    [SerializeField] private GameObject spinDoctorLevelSliderFill;
    public float SpinDoctorUsagePerSecond { get; private set; }
    private static readonly int SPIN_DOCTOR_RPM_PER_LEVEL = 100;
    private float spinDoctorLeft;

    [SerializeField] private Slider shieldLevelSlider;
    [SerializeField] private GameObject shieldLevelSliderFill;
    private static readonly long SHIELD_AMOUNT_PER_LEVEL = 1000000;
    private long shieldLeft;

    public int ScoopedEnergy { get; set; } = 0;

    private readonly string[] casualtiesTexts = {
        "{0} died ({1:0}%)",
        "{0} killed ({1:0}%)",
        "{0} squashed ({1:0}%)",
        "{0} evaporated ({1:0}%)",
        "{0} lost ({1:0}%)",
        "{0} are no more ({1:0}%)",
        "{0} are now ex-people ({1:0}%)",
        "{0} are poorly ({1:0}%)",
        "{0} need some milk ({1:0}%)",
        "{0} have a bad feeling about this ({1:0}%)",
        "{0} did redeem, ma'am ({1:0}%)",
        "{0} have no fun ({1:0}%)",
        "{0} perished ({1:0}%)",
        "{0} don't get no respect ({1:0}%)",
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

        StartCoroutine(DoEverySecondActions());

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

        // EnergyShield
        int shieldLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.EnergyShield).Level;
        if (shieldLevel == 0)
        {
            shieldLevelSliderFill.SetActive(false);
        }
        else
        {
            shieldLeft = shieldLevel * SHIELD_AMOUNT_PER_LEVEL;
            shieldLevelSlider.minValue = 0f;
            shieldLevelSlider.maxValue = shieldLeft;
            shieldLevelSlider.value = shieldLeft;
        }
    }

    public bool ShieldAvailable()
    {
        return shieldLeft > 0;
    }

    IEnumerator DoEverySecondActions()
    {

        int androidSlaveryLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.AndroidSlavery).Level;
        long slaveryIncrease = androidSlaveryLevel * 100;

        int vyagraEnergizingTherapyLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.VyagraEnergizingTherapy).Level;
        float vyagraEnergizingTherapyPercent = vyagraEnergizingTherapyLevel / 100f;

        int previousScoopedEnergy = ScoopedEnergy;

        while (true)
        {
            if (Player.Dead) break;

            // AndroidSlavery

            if (slaveryIncrease > 0)
            {
                currentPopulation += slaveryIncrease;
                string text = string.Format("+{0}", slaveryIncrease);
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                UpdateLabels();
            }

            // VyagraEnergizingTherapy

            long vyagraIncrease = (long)(currentPopulation * vyagraEnergizingTherapyPercent);
            if (vyagraIncrease > 0)
            {
                currentPopulation += vyagraIncrease;
                string text = "+" + vyagraIncrease;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                UpdateLabels();
            }

            // Add scooped energy if it is still the same as previous, to debounce
            if (ScoopedEnergy > 0 && ScoopedEnergy == previousScoopedEnergy)
            {
                AddEnergyCollectedText(ScoopedEnergy);
                UpdateLabels();
                ScoopedEnergy = 0;
            }

            previousScoopedEnergy = ScoopedEnergy;

            yield return EVERY_SECOND;
        }
    }


    void Update()
    {
        if (Player.Dead) return;

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

            if (!Mathf.Approximately(spinDoctorLevelSlider.value, spinDoctorLeft))
            {
                // this is in "if" because we don't want to reset the value every frame, maybe causing 
                // canvas rebuild
                spinDoctorLevelSlider.SetValueWithoutNotify(spinDoctorLeft);
            }

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
        populationLabel.SetText("POP: {0}", currentPopulation);
        gateCounterLabel.SetText("{0}", GateCount);
        energyLabel.SetText(" {0} GW", GameManager.Instance.CollectedEnergy);
        UpdateRpmLabel();
    }

    private void UpdateRpmLabel()
    {
        rpmLabel.SetText("RPM: {0}", (int)Mathf.Abs(Player.GetRpm()));
    }

    // Kills <hitFraction> of the population, considers shield and min casualties. Returns the actual number of dead people.
    public long TakeHit(float hitFraction, long minCasualties, bool addLossText = true)
    {
        long peopleDied = (long)(currentPopulation * hitFraction);
        if (peopleDied < minCasualties) peopleDied = minCasualties;

        if (peopleDied > currentPopulation) peopleDied = currentPopulation;

        long newPeopleDied = peopleDied > shieldLeft ? (peopleDied - shieldLeft) : 0;
        long newShieldLeft = shieldLeft > peopleDied ? (shieldLeft - peopleDied) : 0;

        peopleDied = newPeopleDied;
        shieldLeft = newShieldLeft;

        shieldLevelSlider.SetValueWithoutNotify(shieldLeft);
        if (shieldLeft <= 0f)
        {
            shieldLevelSliderFill.SetActive(false);
        }

        if (addLossText && peopleDied > 0)
        {
            // TODO Move this to something higher, it should not be generated here
            AddPopulationLossText(peopleDied);
        }

        KillPopulation(peopleDied);

        return peopleDied;
    }

    private void KillPopulation(long dead)
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

    public void RotationalDamage(long dead)
    {
        AddPopulationLossTextShort(dead);

        KillPopulation(dead);
    }

    public void AddPopulationLossText(long peopleDied, string textFormatOverride = null)
    {
        float diedPercent = Mathf.Floor(peopleDied / (float)(peopleDied + currentPopulation) * 100f);

        string text = textFormatOverride ?? casualtiesTexts[Random.Range(0, casualtiesTexts.Length)];

        text = string.Format(text, peopleDied, diedPercent);

        populationMessagesManager.Spawn(text, fadingMessageCasualtiesColor);
    }

    private void AddPopulationLossTextShort(long peopleDied)
    {
        string text = rotationCasualtiesTexts[Random.Range(0, rotationCasualtiesTexts.Length)];

        text = string.Format(text, peopleDied);

        rpmMessagesManager.Spawn(text, fadingMessageCasualtiesColor);
    }

    public void Death()
    {
        GameManager.Instance.Save();

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return WAIT_2_SECONDS;
        SceneManager.LoadScene("NewMenu");
    }

    public void CollectEnergy()
    {
        GateCount++;

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
        string text = string.Format("+{0}GW", energyAdded);
        energyMessagesManager.Spawn(text, fadingMessageEnergyColor);
    }

}

public enum LevelType
{
    Regular, UfoSwarm
}

