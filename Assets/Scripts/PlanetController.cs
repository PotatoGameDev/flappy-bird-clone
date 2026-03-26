using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    private readonly float RPM_PENALTY_THRESHOLD = 20f;

    private readonly WaitForSeconds EVERY_SECOND = new(1);

    [SerializeField] private bool god;

    // This is the speed param that influences camera movement and other items. That's why it's internal, not private - other code has to know.
    internal float speed = 0f;
    [SerializeField] private float initialSpeed = 5f;

    [SerializeField] private float speedIncrease = 0.1f;
    internal float ToorboBoost { get; set; }

    [SerializeField] private float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer rendr;

    [SerializeField] private float damageFlashingDurationSeconds = 1f;
    [SerializeField] private float shieldFlashingDurationSeconds = 1f;

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

    // Shield
    [SerializeField] private SpriteRenderer shieldRenderer;
    private Coroutine shieldCoroutine;

    // Burning Damage
    [SerializeField] private GameObject burningEffect;
    [SerializeField] private float borderDangerMargin = 1.0f;

    private float currentSunStarCasualties = 0f;
    private float currentBlackHoleCasualties = 0f;

    private float rotationShake = 0f;
    private float boundaryDamageShake = 0f;
    //private float speedShake = 0f;
    //
    private string[] laserHitTexts = {
        "{0} burned",
        "{0} barbequed",
        "{0} got LASIK",
        "{0} smoked",
    };
    private float timeToLaserDamageSummary;
    private long totalLaserDamage = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();
        originalColor = rendr.color;

        GameplayManager.Instance.Player = this;

        SpriteHolder = transform.Find("Sprite");
    }

    void Start()
    {
        burningEffect.SetActive(false);

        // Start updating rotational penalties
        StartCoroutine(UpdateRorationalPenalties());

        // Damage for getting too close to the sun or to the black hole
        StartCoroutine(UpdateOutOfBoundsDamage());

        if (god)
        {
            rb.gravityScale = 0f;
            speed = 0f;
        }
    }

    void Update()
    {
        if (Dead)
        {
            rb.linearVelocity = Vector2.zero;

            return;
        }

        if (god)
            return;

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

            long casualties = GameplayManager.Instance.TakeHit(outOfBoundsDamagePerSecond * Time.deltaTime, 1, false);

            if (casualties > 0)
            {
                if (blackHoleDamage)
                {
                    currentBlackHoleCasualties += casualties;
                }
                else
                {
                    currentSunStarCasualties += casualties;
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
            EffectsManager.Instance.StartSustainedShake(totalShake);
        }
        else
        {
            EffectsManager.Instance.StopSustainedShake();
        }

        // Ufo Swarm Damage:
        if (totalLaserDamage > 0 && timeToLaserDamageSummary <= 0f)
        {
            GameplayManager.Instance.AddPopulationLossText(totalLaserDamage, laserHitTexts, false);
            totalLaserDamage = 0;
        }
        timeToLaserDamageSummary -= Time.deltaTime;
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

                    // for each rpmAbs above threshold we kill 100 people
                    GameplayManager.Instance.RotationalDamage((int)(penaltyRpm * 100));

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
                GameplayManager.Instance.AddPopulationLossText((long)currentSunStarCasualties, new string[] { "{0} deep fried" });
                currentSunStarCasualties = 0;
            }
            if (currentBlackHoleCasualties > 0)
            {
                GameplayManager.Instance.AddPopulationLossText((long)currentBlackHoleCasualties, new string[] { "{0} spaghettified" });
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
        if (ctx.performed)
        {
            currentJumpForce += jumpForce;
        }
    }

    public void OnBack(InputAction.CallbackContext ctx)
    {
        if (Dead) return;
        if (ctx.performed)
        {
            Death();
        }
    }

    // Collisions:

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (Dead) return;

        SoundManager.Instance.PlayRandomHit(hitAudioClips, hitVolume);

        ShowShieldIfAvailable();

        // Calculate hit fraction:
        float maxHitPercent = 100f;
        float maxHitForce = 10f;

        if (collision.gameObject.CompareTag("GatePipe"))
        {
            maxHitForce = 20f;
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            // Nerfing damage, because UFOs seem lighter than the pipes, also it was hard to beat them.
            maxHitForce = 5f;
            maxHitPercent = 10f;
        }

        // Here we get the magnitude clamped by our max:
        float hitForce = Mathf.Clamp(collision.relativeVelocity.magnitude, 0f, maxHitForce);
        // Here we calculate how much of the maxHitPercent we get.
        // If we hit with the maxHitForce we get 1 * maxHitPercent:
        float hitPercent = maxHitPercent * Mathf.Clamp01(hitForce / maxHitForce);
        //This means, that if maxHitPercent is 20, and we hit full force, we get 1 * 20 / 100, meaning 0.2.
        float hitFraction = hitPercent / 100f;

        // This hit fraction will calculate the percent of population, that died.
        long peopleDied = GameplayManager.Instance.TakeHit(hitFraction, 1000);

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
        }
    }

    private void ShowShieldIfAvailable()
    {
        if (shieldCoroutine == null && GameplayManager.Instance.ShieldAvailable())
        {
            shieldCoroutine = StartCoroutine(ShieldFlashing(shieldRenderer));
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
            ShowShieldIfAvailable();

            long killed = GameplayManager.Instance.TakeHit(0.01f, 1000, false);

            if (killed > 0f)
            {
                ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
                explosion.transform.SetParent(SpriteHolder);
                explosion.transform.position = collider.transform.position;
                explosion.transform.localScale = Vector2.one * Random.Range(0.2f, 0.4f);

                totalLaserDamage += killed;
                timeToLaserDamageSummary = 1; // one second after last damage
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Gate"))
        {
            GameplayManager.Instance.CollectEnergy();
            speed += speedIncrease;
        }
    }

    public float GetRpm()
    {
        return rb.angularVelocity / 6f;
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

    private IEnumerator ShieldFlashing(SpriteRenderer r)
    {
        float elapsed = 0f;
        Color originalColor = r.color;

        while (elapsed < shieldFlashingDurationSeconds)
        {
            elapsed += Time.deltaTime;

            Color color = r.color;
            color.a = Mathf.PingPong(Time.time * flashSpeed, 1f);

            r.color = color;
            yield return null;
        }

        r.color = originalColor;
        shieldCoroutine = null;
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
    }

    public void DeathAnimationEnded()
    {
        //Destroy(gameObject);
    }

}
