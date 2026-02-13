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
        UpdateLabels();
    }

    void Update()
    {
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        populationLabel.text = "POP: " + GameManager.Instance.State.Population;
        rpmLabel.text = "RPM: " + GameManager.Instance.Player.GetRpm();
        gateCounterLabel.text = gateCount.ToString();
    }

    public void TakeHit(float force)
    {
        // Calculate casualties:
        float maxHitPercent = 50;
        float maxHitForce = 100;

        float hitPercent = maxHitPercent * Mathf.Clamp01(force / maxHitForce);
        int peopleDied = (int)(GameManager.Instance.State.Population * (hitPercent / 100));
        GameManager.Instance.State.Population -= peopleDied;

        AddPopulationLossText(peopleDied);
    }

    private void AddPopulationLossText(int peopleDied)
    {
        GameObject fadingText = Instantiate(fadingTextPrefab, populationLabel.transform.position, Quaternion.identity, populationLabel.transform.parent);

        FadingTextController ftc = fadingText.GetComponent<FadingTextController>();
        ftc.Init(peopleDied);
    }

    public void Death()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
