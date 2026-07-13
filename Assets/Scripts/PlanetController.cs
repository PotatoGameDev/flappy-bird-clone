using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    private readonly float RPM_PENALTY_THRESHOLD = 20f;
    private readonly float RPM_MAX = 80f; // This is when the damage is maximal

    private readonly WaitForSeconds EVERY_SECOND = new(1);

    [SerializeField] private InputActionReference jumpActionReference;

    [SerializeField] private PauseMenuController pauseMenuController;

    // This is the speed param that influences camera movement and other items. That's why it's internal, not private - other code has to know.
    internal float speed = 0f;
    [SerializeField] private float initialSpeed = 5f;

    [SerializeField] private float speedIncrease = 0.1f;
    internal float ToorboBoost { get; set; }

    [SerializeField] private float jumpForce = 10f;

    // TODO Couldn't have this been just a bool?
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer rendr;

    public Vector2 Velocity => rb.linearVelocity;

    [SerializeField] private float damageFlashingDurationSeconds = 1f;

    private readonly float flashSpeed = 0.2f;

    public Transform SpriteHolder { get; private set; }

    private Color originalColor;

    // Audio sources
    [SerializeField] private float screamsVolume = 0.5f;
    [SerializeField] private float hitVolume = 1f;
    [SerializeField] private float quakeVolume = 0.5f;

    [SerializeField] private AudioClip[] hitAudioClips;
    [SerializeField] private AudioClip[] quakeAudioClips;


    // Particles
    [SerializeField] private ParticleSystem peopleParticleSystem;

    [SerializeField] private ParticleSystem[] spinDoctorParticleSystemsLeft;
    [SerializeField] private ParticleSystem[] spinDoctorParticleSystemsRight;

    // Burning Damage
    [SerializeField] private GameObject burningEffect;
    [SerializeField] private float borderDangerMargin = 1.0f;

    private float currentSunStarCasualties = 0f;
    private float currentBlackHoleCasualties = 0f;

    private float rotationShake = 0f;
    private float boundaryDamageShake = 0f;

    private const int LaserHitTextsCount = 4;
    private const string LaserHitTextsPrefix = "casualties_laser";
    private const int BlackHoleTextsCount = 1;
    private const string BlackHoleTextsPrefix = "casualties_black_hole";
    private const int SunStarTextsCount = 1;
    private const string SunStarTextsPrefix = "casualties_sun_star";

    private float timeToLaserDamageSummary;
    private long totalLaserDamage = 0;

    // Boss
    [SerializeField] private BossManager bossManager;

    // Other
    [SerializeField] private EnergyShieldController shieldController;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();
        originalColor = rendr.color;

        GameplayManager.Instance.Player = this;

        // Selecting the sprite
        SpriteHolder = transform.Find("Sprite");

        string selectedPlanetName = "planet0" + GameManager.Instance.PlanetType.ToString();

        for (int i = 0; i < SpriteHolder.childCount; i++)
        {
            Transform child = SpriteHolder.GetChild(i);
            child.gameObject.SetActive(child.name == selectedPlanetName);
        }
    }

    void Start()
    {
        burningEffect.SetActive(false);

        // Start updating rotational penalties
        StartCoroutine(UpdateRorationalPenalties());

        // Damage for getting too close to the sun or to the black hole
        StartCoroutine(UpdateOutOfBoundsDamage());

    }

    void OnEnable()
    {
        jumpActionReference.action.performed += OnJump;
    }

    void OnDisable()
    {
        jumpActionReference.action.performed -= OnJump;
    }

    void FixedUpdate()
    {
        if (pauseMenuController.IsPaused)
        {
            return;
        }
        if (Dead)
        {
            rb.linearVelocity = Vector2.zero;

            return;
        }

        if (speed == 0f)
        {
            // This is to manage the problem of FixedUpdate in camera script running before this method.
            // That caused camera to be initially slightly faster than the planet... 
            // This way, the camera starts with 0 speed.
            speed = initialSpeed;
        }

        float effectiveSpeed = speed + ToorboBoost;

        // Shake for speed, max 1.0 for speed 30. start with 7.
        // Actually lets not add speed shake:
        //speedShake = Mathf.Lerp(0f, 1f, (effectiveSpeed - 7f) / 30f);

        if (currentJumpForce > 0f)
        {
            rb.linearVelocity = new Vector2(effectiveSpeed, currentJumpForce);
            currentJumpForce = 0f;
        }
        else
        {
            rb.linearVelocity = new Vector2(effectiveSpeed, rb.linearVelocity.y);
        }


        // Calculating casualties due to being too close to the boundary
        float height = Camera.main.orthographicSize * 2f;
        boundaryDamageShake = 0f;

        // TODO If i switch to moving camera (that follows the player) then there might be a problem
        float deathHeightTop = Camera.main.transform.position.y + height / 2;
        float dangerHeightTop = deathHeightTop - borderDangerMargin;

        float deathHeightBottom = Camera.main.transform.position.y - height / 2;
        float dangerHeightBottom = deathHeightBottom + borderDangerMargin;

        float outOfBoundsDamagePerSecond = 0f;
        bool blackHoleDamage = false;

        if (transform.position.y <= dangerHeightBottom)
        {
            // The planet is below damage threshold, should start getting damage
            //
            float distance = Mathf.Abs(transform.position.y - dangerHeightBottom);
            outOfBoundsDamagePerSecond = distance / borderDangerMargin;
            blackHoleDamage = false;
        }
        else if (transform.position.y >= dangerHeightTop)
        {
            // The planet is above danger threshold, should start getting damage
            //
            float distance = Mathf.Abs(transform.position.y - dangerHeightTop);

            outOfBoundsDamagePerSecond = distance / borderDangerMargin;
            blackHoleDamage = true;
        }

        if (outOfBoundsDamagePerSecond > 0f)
        {
            // We lerp from the original color to the half of the red (so not so much red)
            rendr.color = Color.Lerp(originalColor, Color.Lerp(originalColor, Color.red, 0.5f), outOfBoundsDamagePerSecond);

            long peopleDied = GameplayManager.Instance.TakeHit(HitType.BorderProximity, outOfBoundsDamagePerSecond * Time.fixedDeltaTime);

            if (peopleDied > 0)
            {
                if (blackHoleDamage)
                {
                    currentBlackHoleCasualties += peopleDied;
                }
                else
                {
                    currentSunStarCasualties += peopleDied;
                }

                // Adding sustained shake, up to 1.0 for 5000 casualties
                // We add it here to simplify ifs, one of them will be 0f.
                boundaryDamageShake = Mathf.Lerp(
                        0,
                        1,
                        (currentSunStarCasualties + currentBlackHoleCasualties) / 5000
                );

                SoundManager.Instance.PlayScreams(screamsVolume);
            }
            burningEffect.SetActive(true);
        }
        else
        {
            burningEffect.SetActive(false);
            rendr.color = originalColor;
        }

        float totalShake = boundaryDamageShake + rotationShake; // + speedShake;
        totalShake = Mathf.Clamp(totalShake, 0f, 2f);
        if (totalShake > 0f)
        {
            EffectsManager.Instance.StartSustainedShake(
                    ShakeSource.BoundaryDamage,
                    totalShake
                    );
        }
        else
        {
            EffectsManager.Instance.StopSustainedShake(
                    ShakeSource.BoundaryDamage
                    );
        }

        // Ufo Swarm Damage:
        if (totalLaserDamage > 0 && timeToLaserDamageSummary <= 0f)
        {
            GameplayManager.Instance.AddPopulationLossText(totalLaserDamage, LaserHitTextsPrefix, LaserHitTextsCount, false);
            totalLaserDamage = 0;
        }
        timeToLaserDamageSummary -= Time.fixedDeltaTime;
    }

    private IEnumerator UpdateRorationalPenalties()
    {
        while (true)
        {
            float rpmAbs = Mathf.Abs(GetRpm());
            float penaltyRpm = 0f;
            rotationShake = 0f;

            foreach (ParticleSystem psl in spinDoctorParticleSystemsLeft)
            {
                var sdEmissionLeft = psl.emission;
                sdEmissionLeft.rateOverTime = 0;
            }
            foreach (ParticleSystem psr in spinDoctorParticleSystemsRight)
            {
                var sdEmissionRight = psr.emission;
                sdEmissionRight.rateOverTime = 0;
            }

            if (rpmAbs > 0f)
            {
                if (rpmAbs > RPM_PENALTY_THRESHOLD)
                {
                    penaltyRpm = rpmAbs - RPM_PENALTY_THRESHOLD;

                    // for each rpmAbs above threshold we kill people
                    if (penaltyRpm > 0)
                    {

                        GameplayManager.Instance.RotationalDamage(
                                rpmAbs / RPM_MAX
                                );
                    }

                    // Add shake, max 1 for 50RPM, starting with 10 RPM 
                    rotationShake = Mathf.Lerp(0, 1, (penaltyRpm - 10) / 50f);
                }

                float rpmDamped = GameplayManager.Instance.SpinDoctorUsagePerSecond;
                if (rb.angularVelocity < 0f)
                {
                    foreach (ParticleSystem psl in spinDoctorParticleSystemsLeft)
                    {
                        var sdEmission = psl.emission;
                        sdEmission.rateOverTime = rpmDamped;
                    }
                }
                else
                {
                    foreach (ParticleSystem psr in spinDoctorParticleSystemsRight)
                    {
                        var sdEmission = psr.emission;
                        sdEmission.rateOverTime = rpmDamped;
                    }
                }
            }

            var emission = peopleParticleSystem.emission;
            emission.rateOverTime = penaltyRpm;

            yield return EVERY_SECOND;
        }
    }

    private IEnumerator UpdateOutOfBoundsDamage()
    {
        while (true)
        {
            if (currentSunStarCasualties > 0)
            {
                GameplayManager.Instance.AddPopulationLossText(
                        (long)currentSunStarCasualties,
                        SunStarTextsPrefix,
                        SunStarTextsCount
                        );
                currentSunStarCasualties = 0;
            }
            if (currentBlackHoleCasualties > 0)
            {
                GameplayManager.Instance.AddPopulationLossText(
                        (long)currentBlackHoleCasualties,
                        BlackHoleTextsPrefix,
                        BlackHoleTextsCount
                        );
                currentBlackHoleCasualties = 0;
            }
            yield return EVERY_SECOND;
        }
    }

    // Life
    public bool Dead { get; set; }

    // Controls:

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (pauseMenuController.IsPaused || Dead)
        {
            return;
        }
        currentJumpForce += jumpForce;

        shieldController.ActivateJumpShield();
    }

    // Collisions:

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (Dead) return;

        // Calculate hit fraction:

        HitType hitType = HitType.GateCollision;

        float maxHitForce;
        float maxHitPercent;
        bool parried;

        if (collision.gameObject.CompareTag("GatePipe"))
        {
            maxHitForce = 20f;
            maxHitPercent = 100f;
            parried = false;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // Enemy is the small flying saucer
            hitType = HitType.BossCollision;

            FlyingSaucerController flyingSaucer = collision.gameObject.GetComponent<FlyingSaucerController>();
            if (flyingSaucer.state != FlyingSaucerState.DEAD)
            {
                parried = shieldController.TryParry(flyingSaucer);
                maxHitForce = 10f;
                maxHitPercent = 100f;
            }
            else
            {
                maxHitForce = 0f;
                maxHitPercent = 0f;
                parried = false;
            }
        }
        else if (collision.gameObject.CompareTag("Mothershipper"))
        {
            hitType = HitType.Mothershipper;

            MotherShipperController motherShipper = collision.gameObject.GetComponent<MotherShipperController>();

            parried = shieldController.TryParry(motherShipper);
            maxHitForce = 20f;
            maxHitPercent = 100f;
        }
        else if (collision.gameObject.CompareTag("FinalBoss"))
        {
            hitType = HitType.FinalBoss;

            // Slowing down after every boss hit to give space for next sequence
            speed = initialSpeed;


            FinalBossController finalBoss = collision.gameObject.GetComponent<FinalBossController>();
            parried = shieldController.TryParry(finalBoss);

            maxHitForce = 20f;
            maxHitPercent = 100f;
        }
        else if (collision.gameObject.CompareTag("Rocket"))
        {
            hitType = HitType.Rocket;

            RocketController rocket = collision.gameObject.GetComponent<RocketController>();

            parried = shieldController.TryParry(rocket);

            if (rocket.type == RocketController.RocketType.Tiny)
            {
                maxHitForce = 1f;
                maxHitPercent = 2f;
            }
            else
            {
                maxHitForce = 5f;
                maxHitPercent = 10f;
            }
        }
        else if (collision.gameObject.CompareTag("Trash"))
        {
            maxHitForce = 5f;
            maxHitPercent = 10f;
            parried = false;
        }
        else
        {
            throw new System.Exception("Unknown hit: " + collision.gameObject.tag + " - " + collision.gameObject.name);
        }

        SoundManager.Instance.PlayRandomHit(hitAudioClips, hitVolume);

        shieldController.ActivateProtectShield();

        // Here we get the magnitude clamped by our max:
        float hitForce = Mathf.Clamp(collision.relativeVelocity.magnitude, 0f, maxHitForce);
        // Here we calculate how much of the maxHitPercent we get.
        // If we hit with the maxHitForce we get 1 * maxHitPercent:
        float hitPercent = maxHitPercent * Mathf.Clamp01(hitForce / maxHitForce);
        //This means, that if maxHitPercent is 20, and we hit full force, we get 1 * 20 / 100, meaning 0.2.
        float hitFraction = hitPercent / 100f;

        // This hit fraction will calculate the percent of population, that died.
        long peopleDied = GameplayManager.Instance.TakeHit(hitType, hitFraction, parried);

        // Shaking the screen in direction of the collision
        // TODO: We should pass the shake in percent actually! RelativeVelocity means nothing to shake.
        // % means everything, like 100% means crazy shake, meanwhile 20% is mid.:w
        EffectsManager.Instance.Shake(collision.relativeVelocity);

        if (peopleDied > 0)
        {
            SoundManager.Instance.PlayScreams(screamsVolume);
            SoundManager.Instance.PlayRandomQuake(quakeAudioClips, quakeVolume);

            StartCoroutine(DamageFlashing(rendr));

            peopleParticleSystem.Emit((int)Mathf.Log10(peopleDied));

            GameplayManager.Instance.AddPopulationLossText(peopleDied);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (Dead) return;
        if (collider.CompareTag("BoundaryBack") || collider.CompareTag("BlackHoleBoundary") || collider.CompareTag("SunStarBoundary"))
        {
            Death();
            return;
        }

        if (collider.CompareTag("Bullet"))
        {
            shieldController.ActivateProtectShield();

            long peopleDied = GameplayManager.Instance.TakeHit(HitType.BossLaser);
            if (peopleDied > 0f)
            {
                ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
                explosion.transform.SetParent(SpriteHolder);

                explosion.transform.position = collider.transform.position;
                explosion.transform.localScale = Vector2.one * Random.Range(0.2f, 0.4f);

                totalLaserDamage += peopleDied;
                timeToLaserDamageSummary = 1; // one second after last damage

                EffectsManager.Instance.Shake(0.5f);
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Gate"))
        {
            GameplayManager.Instance.PassGate();
            speed += speedIncrease;
        }
    }

    public float GetRpm()
    {
        return rb.angularVelocity / 6f;
    }

    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public void AddRpm(float rpm)
    {
        rb.angularVelocity += rpm * 6f;
    }

    private IEnumerator DamageFlashing(SpriteRenderer r)
    {
        float flashSpeed = 8f;
        float elapsed = 0f;

        while (elapsed < damageFlashingDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * flashSpeed, 1f);

            // We lerp from the original color to the half of the red (so not so much red)
            r.color = Color.Lerp(originalColor, Color.Lerp(originalColor, Color.red, 0.5f), t);
            yield return null;
        }

        r.color = originalColor;
    }


    public void Death()
    {
        // Letting know other components that the player died
        Dead = true;

        burningEffect.SetActive(true);

        // Hiding the player
        rendr.enabled = false;

        // Handling death logic
        GameplayManager.Instance.Death();

        // Explosion animation for fun
        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
        explosion.transform.SetParent(transform);
        explosion.transform.localPosition = Vector2.zero;
        explosion.transform.localScale = Vector2.one * 2f;

        explosion.OnFinished.AddListener(DeathAnimationEnded);

        SoundManager.Instance.PlayExplosion();
    }

    public void DeathAnimationEnded()
    {
        burningEffect.SetActive(false);
        //Destroy(gameObject);
    }

}
