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

    private long currentPopulation = 0;

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
        currentPopulation = GameManager.Instance.GetPopulationForLevel(GameManager.Instance.State.CivTypePassed);

        UpdateLabels();
    }

    void Update()
    {
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
        float maxHitPercent = 50;
        float maxHitForce = 100;

        float hitPercent = maxHitPercent * Mathf.Clamp01(force / maxHitForce);
        long peopleDied = (long)(currentPopulation * (hitPercent / 100));
        currentPopulation -= peopleDied;

        AddPopulationLossText(peopleDied);
    }

    private void AddPopulationLossText(long peopleDied)
    {
        GameObject fadingText = Instantiate(fadingTextPrefab, populationLabel.transform.position, Quaternion.identity, populationLabel.transform.parent);

        FadingTextController ftc = fadingText.GetComponent<FadingTextController>();
        ftc.Init(peopleDied);
    }

    public void Death()
    {
        GameManager.Instance.Save();
        SceneManager.LoadScene("NewMenu");
    }
}
