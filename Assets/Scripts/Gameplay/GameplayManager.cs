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

    private static readonly WaitForSeconds WAIT_2_SECONDS = new(2f);
    private static readonly WaitForSeconds WAIT_1_SECOND = new(1f);

    [SerializeField] private TextMeshProUGUI populationLabel;
    [SerializeField] private TextMeshProUGUI rpmLabel;
    [SerializeField] private GameObject gateCounterContainer;
    [SerializeField] private TextMeshProUGUI gateCounterLabel;
    [SerializeField] private TextMeshProUGUI energyLabel;

    public int GateCount { get; set; } = 0;

    [SerializeField] private ToorboBoostController toorboBoostController;

    [SerializeField] private Color fadingMessageCasualtiesColor;
    [SerializeField] private Color fadingMessageEnergyColor;
    [SerializeField] private Color fadingMessagePopulationAddedColor;

    [SerializeField] private FadingMessagesManager populationMessagesManager;
    [SerializeField] private FadingMessagesManager energyMessagesManager;
    [SerializeField] private FadingMessagesManager rpmMessagesManager;

    private long _currentPopulation = 0;
    private long CurrentPopulation
    {
        get
        {
            return _currentPopulation;
        }

        set
        {
            if (value < 0) return;
            _currentPopulation = value;
        }
    }

    private readonly static WaitForSeconds EVERY_SECOND = new(1f);

    [SerializeField] private Slider spinDoctorLevelSlider;
    [SerializeField] private GameObject spinDoctorLevelSliderFill;
    public float SpinDoctorUsagePerSecond { get; private set; }
    private float spinDoctorLeft;

    [SerializeField] private Slider shieldLevelSlider;
    [SerializeField] private GameObject shieldLevelSliderFill;
    private long shieldLeft;


    [SerializeField] private Slider energyGoalSlider;

    [Header("Boss")]
    [SerializeField] private TextMeshProUGUI bossLabel;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private GameObject bossHealthSliderFill;
    [SerializeField] private GameObject bossHealthBarContainer;
    [SerializeField] private BossManager bossManager;

    [SerializeField] private GameObject winAccomplishedPanel;

    private long scoopedEnergy = 0;

    public void AddScoopedEnergy(long amount)
    {
        // This is for the upgrades:
        GameManager.Instance.CollectedEnergy += amount;

        // This is for the energy floating text to show up after a debounce:
        scoopedEnergy += amount;

        // This is to refill shields and fill boss goal bar:
        AddCollectedEnergy(amount);

        // And finally we update UI:
        UpdateEnergyGoalSlider();
    }

    public long collectedEnergy = 0;

    public void AddCollectedEnergy(long amount)
    {
        if (bossManager.IsBossActive())
        {
            long maxEnergyShield = UpgradesManager.Instance.GetEnergyShieldMax();
            if (shieldLeft < maxEnergyShield)
            {
                long shieldToRefill = maxEnergyShield - shieldLeft;
                if (shieldToRefill > amount)
                {
                    shieldToRefill = amount;
                }
                amount -= shieldToRefill;
                shieldLeft += shieldToRefill;

                shieldLevelSlider.SetValueWithoutNotify(shieldLeft);
                shieldLevelSliderFill.SetActive(true);
            }

            float maxSpinDoctorRpmPerSec = UpgradesManager.Instance.GetSpinDoctorMaxRpmPerSecond();
            if (amount > 0 && spinDoctorLeft < maxSpinDoctorRpmPerSec)
            {
                float spinDoctorToRefill = maxSpinDoctorRpmPerSec - spinDoctorLeft;
                if (spinDoctorToRefill > amount)
                {
                    spinDoctorToRefill = amount;
                }
                // TODO This should be somehow converted between energy <> RPM
                amount -= (long)spinDoctorToRefill;
                spinDoctorLeft += spinDoctorToRefill;

                spinDoctorLevelSlider.SetValueWithoutNotify(spinDoctorLeft);
                spinDoctorLevelSliderFill.SetActive(true);
            }
            // TODO The same here, this should be somehow converted between energy <> a second of toorbo 
            long toorboMax = (long)toorboBoostController.MaxToorboBoost;
            long toorboLeft = (long)toorboBoostController.ToorboBoostLeft;
            if (amount > 0 && toorboLeft < toorboMax)
            {
                long toorboRefill = toorboMax - toorboLeft;
                if (toorboRefill > amount)
                {
                    toorboRefill = amount;
                }
                amount -= toorboRefill;
                toorboBoostController.ToorboBoostLeft += toorboRefill;
                toorboBoostController.UpdateSlider();
            }
        }
        else
        {
            collectedEnergy += amount;

            if (collectedEnergy >= GameManager.Instance.GetAdvanceEnergy())
            {
                if (!bossManager.IsBossActive())
                {
                    bossManager.ActivateBoss();
                    gateCounterContainer.SetActive(false);
                    bossHealthBarContainer.SetActive(true);
                }
            }
        }
    }

    private const string CasualtiesTextsTableName = "Casualties_Strings";
    private const int CasualtiesTextsCount = 14;
    private const int RotationCasualtiesTextsCount = 4;

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
        CurrentPopulation = GameManager.Instance.GetBasePopulation();

        UpdateLabels();

        StartCoroutine(DoEverySecondActions());

        // SpinDoctor
        int spinDoctorLeft = UpgradesManager.Instance.GetSpinDoctorMaxRpmPerSecond();

        if (spinDoctorLeft == 0)
        {
            spinDoctorLevelSliderFill.SetActive(false);
        }
        else
        {
            spinDoctorLevelSlider.minValue = 0f;
            spinDoctorLevelSlider.maxValue = spinDoctorLeft;
            spinDoctorLevelSlider.value = spinDoctorLeft;
        }

        // EnergyShield
        long shieldLeft = UpgradesManager.Instance.GetEnergyShieldMax();
        if (shieldLeft == 0)
        {
            shieldLevelSliderFill.SetActive(false);
        }
        else
        {
            shieldLevelSlider.minValue = 0f;
            shieldLevelSlider.maxValue = shieldLeft;
            shieldLevelSlider.value = shieldLeft;
        }

        switch (GameManager.Instance.CurrentLevel)
        {
            case 0:
                bossLabel.SetText(Loc.Get("gameplay_boss_name_1"));
                break;
            case 1:
                bossLabel.SetText("TODO");
                break;
            case 2:
                bossLabel.SetText("TODO");
                break;
            default:
                break;
        }
        gateCounterContainer.SetActive(true);
        bossHealthBarContainer.SetActive(false);

        energyGoalSlider.minValue = 0;
        energyGoalSlider.maxValue = GameManager.Instance.GetAdvanceEnergy();
        energyGoalSlider.value = 0;
    }

    public void SetBossHealth(float health, float maxHealth)
    {
        if (health != bossHealthSlider.maxValue)
        {
            bossHealthSlider.minValue = 0f;
            bossHealthSlider.maxValue = maxHealth;
        }
        bossHealthSlider.SetValueWithoutNotify(health);
        if (Mathf.Approximately(health, 0f))
        {
            // Player won!
            bossHealthSliderFill.SetActive(false);

            StartCoroutine(BossFightWinSequence());

        }
    }

    private IEnumerator BossFightWinSequence()
    {
        yield return WAIT_2_SECONDS;
        yield return WAIT_1_SECOND;

        winAccomplishedPanel.SetActive(true);

        yield return WAIT_2_SECONDS;
        yield return WAIT_1_SECOND;

        GameManager.Instance.UnlockNextPhase();

        SceneManager.LoadScene("NewMenu");
    }

    public bool ShieldAvailable()
    {
        return shieldLeft > 0;
    }

    IEnumerator DoEverySecondActions()
    {
        long slaveryIncrease = UpgradesManager.Instance.GetPopulationNumberPerSecond(UpgradeId.AndroidSlavery);
        long previousScoopedEnergy = scoopedEnergy;

        while (true)
        {
            if (Player.Dead) break;

            bool updateLabels = false;

            // AndroidSlavery

            if (slaveryIncrease > 0)
            {
                CurrentPopulation += slaveryIncrease;
                string text = string.Format("+{0}", slaveryIncrease);
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                updateLabels = true;
            }

            // VyagraEnergizingTherapy

            float vyagraIncrease = UpgradesManager.Instance.GetPopulationPercentPerSecond(UpgradeId.VyagraEnergizingTherapy);
            if (vyagraIncrease > 0)
            {
                long totalIncrease = (long)(CurrentPopulation * vyagraIncrease);
                CurrentPopulation += totalIncrease;
                string text = "+" + totalIncrease;
                populationMessagesManager.Spawn(text, fadingMessagePopulationAddedColor);
                updateLabels = true;
            }

            // Add scooped energy if it is still the same as previous, to debounce
            if (scoopedEnergy > 0 && scoopedEnergy == previousScoopedEnergy)
            {
                AddEnergyCollectedText(scoopedEnergy);
                updateLabels = true;
                scoopedEnergy = 0;
            }

            previousScoopedEnergy = scoopedEnergy;

            if (updateLabels)
            {
                UpdateLabels();
            }

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
        populationLabel.SetText("POP: " + FormatBigNumber(CurrentPopulation));

        if (gateCounterContainer.activeInHierarchy)
        {
            gateCounterLabel.SetText("{0}", GateCount);
        }
        energyLabel.SetText(" {0} GW", GameManager.Instance.CollectedEnergy);
        UpdateRpmLabel();
    }

    private void UpdateEnergyGoalSlider()
    {
        energyGoalSlider.SetValueWithoutNotify(collectedEnergy);
    }

    private void UpdateRpmLabel()
    {
        rpmLabel.SetText("RPM: {0}", (int)Mathf.Abs(Player.GetRpm()));
    }

    public long TakeHit(HitType type, float fraction = 1f)
    {
        fraction = Mathf.Clamp01(fraction);

        long peopleDied = type switch
        {
            HitType.Spin => (long)(fraction * 1_000_000L),
            HitType.BorderProximity => (long)(fraction * 1_000_000L),
            HitType.BossCollision => (long)(fraction * 1_000_000L),
            HitType.BossLaser => 1_000L,
            HitType.GateCollision => 1_000_000L,
            _ => 0L
        };

        if (peopleDied > CurrentPopulation) peopleDied = CurrentPopulation;

        long newPeopleDied = peopleDied > shieldLeft
            ? (peopleDied - shieldLeft)
            : 0;
        long newShieldLeft = shieldLeft > peopleDied
            ? (shieldLeft - peopleDied)
            : 0;
        peopleDied = newPeopleDied;
        shieldLeft = newShieldLeft;

        shieldLevelSlider.SetValueWithoutNotify(shieldLeft);
        if (shieldLeft <= 0f)
        {
            shieldLevelSliderFill.SetActive(false);
        }

        KillPopulation(peopleDied);

        return peopleDied;
    }

    private void KillPopulation(long dead)
    {
        if (CurrentPopulation <= dead)
        {
            CurrentPopulation = 0;
        }
        else
        {
            CurrentPopulation -= dead;
        }

        UpdateLabels();

        if (CurrentPopulation <= 0)
        {
            Player.Death();
        }
    }

    public void RotationalDamage(float fraction)
    {
        long peopleDied = TakeHit(HitType.Spin, fraction);

        AddPopulationLossTextShort(peopleDied);
    }

    public void AddPopulationLossText(long peopleDied, string prefixOverride = "casualties", int countOverride = CasualtiesTextsCount, bool showPercent = true)
    {
        float diedPercent = Mathf.Floor(peopleDied / (float)(peopleDied + CurrentPopulation) * 100f);

        string text;

        if (showPercent)
        {
            text = Loc.GetRandom(
                    prefixOverride,
                    countOverride,
                    CasualtiesTextsTableName,
                    FormatBigNumber(peopleDied),
                    diedPercent
                    );
        }
        else
        {
            text = Loc.GetRandom(
                    prefixOverride,
                    countOverride,
                    CasualtiesTextsTableName,
                    FormatBigNumber(peopleDied)
                    );
        }

        populationMessagesManager.Spawn(text, fadingMessageCasualtiesColor);
    }

    private void AddPopulationLossTextShort(long peopleDied)
    {
        string text = Loc.GetRandom(
                "casualties_rotation",
                RotationCasualtiesTextsCount,
                CasualtiesTextsTableName,
                FormatBigNumber(peopleDied)
                );

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

    public void PassGate()
    {
        GateCount++;

        UpdateLabels();
    }

    private void AddEnergyCollectedText(long energyAdded)
    {
        string text = string.Format("+{0}GW", energyAdded);
        energyMessagesManager.Spawn(text, fadingMessageEnergyColor);
    }

    private string FormatBigNumber(long value)
    {
        if (value >= 1_000_000_000_000) return (value / 1_000_000_000_000).ToString("F1") + "T";
        if (value >= 1_000_000_000) return (value / 1_000_000_000).ToString("F1") + "B";
        //if (value >= 1_000_000) return (value / 1_000_000_000_000).ToString("F1") + "M";
        //if (value >= 1_000) return (value / 1_000_000_000_000).ToString("F1") + "K";
        return value.ToString("N0");
    }

}

public enum HitType
{
    GateCollision,
    BossCollision,
    BossLaser,
    BorderProximity,
    Spin
}
