using UnityEngine;
using UnityEngine.UI;

public class ToorboBoostController : MonoBehaviour
{
    [SerializeField] private PlanetController planet;
    [SerializeField] private Camera cam;

    [SerializeField] private ParticleSystem topParticles;
    [SerializeField] private ParticleSystem bottomParticles;


    [SerializeField] private float toorboBoostSpeedAdd = 0.5f;

    private float targetPlanetToCameraOffset;

    public float MaxToorboBoost { get; set; }
    public float ToorboBoostLeft { get; set; }

    [SerializeField] private Slider toorboBoostSlider;
    [SerializeField] private GameObject toorboBoostSliderFill;

    [SerializeField] private LayerMask obstacleLayer;


    void Start()
    {
        int boostLevel = UpgradesManager.Instance.GetUpgrade(UpgradeId.ToorboBoost).Level;

        // Slider
        if (boostLevel == 0)
        {
            toorboBoostSliderFill.SetActive(false);
        }
        else
        {
            MaxToorboBoost = UpgradesManager.Instance.GetToorboBoostSecondsForLevel();
            ToorboBoostLeft = MaxToorboBoost;
            toorboBoostSlider.minValue = 0f;
            toorboBoostSlider.maxValue = ToorboBoostLeft;
            toorboBoostSlider.value = ToorboBoostLeft;
        }

        targetPlanetToCameraOffset = PlanetBehindCamera();
    }

    private float PlanetBehindCamera()
    {
        return cam.transform.position.x - planet.transform.position.x;
    }

    void Update()
    {
        if (planet.Dead) return;

        transform.position = planet.transform.position;

        if (ToorboBoostLeft <= 0f)
        {
            toorboBoostSliderFill.SetActive(false);
            planet.ToorboBoost = 0f;
            return;
        }
        else
        {
            toorboBoostSliderFill.SetActive(true);
        }


        if (PlanetBehindCamera() > targetPlanetToCameraOffset && HasRoomForToorbo())
        {
            SetEmission(topParticles, 20);
            SetEmission(bottomParticles, 20);

            planet.ToorboBoost = toorboBoostSpeedAdd;

            ToorboBoostLeft -= Time.deltaTime;

            UpdateSlider();
        }
        else
        {
            SetEmission(topParticles, 0);
            SetEmission(bottomParticles, 0);

            planet.ToorboBoost = 0f;
        }
    }

    public void UpdateSlider()
    {
        if (!Mathf.Approximately(toorboBoostSlider.value, ToorboBoostLeft))
        {
            // this is in "if" because we don't want to reset the value every frame, maybe causing 
            // canvas rebuild
            toorboBoostSlider.SetValueWithoutNotify(ToorboBoostLeft);
        }
    }

    private void SetEmission(ParticleSystem system, int target)
    {
        if (target > 0)
        {
            if (!system.isPlaying)
            {
                system.Play();
            }
        }
        else
        {
            if (system.isPlaying)
            {
                system.Stop();
            }
        }

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = target;
    }

    private bool HasRoomForToorbo()
    {
        RaycastHit2D hit = Physics2D.Raycast(planet.transform.position, Vector2.right, 2f, obstacleLayer);

        return hit.collider == null;
    }
}
