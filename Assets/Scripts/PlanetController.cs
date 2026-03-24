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

    [SerializeField]
    private float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer rendr;

    [SerializeField]
    private float invincibilityDurationSeconds = 2f;

    private readonly float flashSpeed = 0.2f;
    private bool invincible = false;


    Color originalColor;

    // Audio sources
    [SerializeField]
    private float screamsVolume = 0.5f;
    [SerializeField]
    private float hitVolume = 1f;
    [SerializeField]
    private float quakeVolume = 0.5f;

    [SerializeField]
    private AudioClip[] hitAudioClips;
    [SerializeField]
    private AudioClip[] quakeAudioClips;

    // Animations
    [SerializeField] private Animator explosionAnimation;

    // Particles
    [SerializeField] private ParticleSystem peopleParticleSystem;

    [SerializeField] private ParticleSystem[] spinDoctorParticleSystemsLeft;
    [SerializeField] private ParticleSystem[] spinDoctorParticleSystemsRight;

    // Shield
    [SerializeField] private SpriteRenderer shieldRenderer;

    // Burning Damage
    [SerializeField] private GameObject burningEffect;
    [SerializeField] private float borderDangerMargin = 1.0f;

    private float currentSunStarCasualties = 0f;
    private float currentBlackHoleCasualties = 0f;

    private float rotationShake = 0f;
    private float boundaryDamageShake = 0f;
    private float speedShake = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();
        originalColor = rendr.color;

        GameplayManager.Instance.Player = this;
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

        float totalShake = boundaryDamageShake + rotationShake + speedShake;
        totalShake = Mathf.Clamp(totalShake, 0f, 2f);
        if (totalShake > 0f)
        {
            EffectsManager.Instance.StartSustainedShake(totalShake);
        }
        else
        {
            EffectsManager.Instance.StopSustainedShake();
        }
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
                GameplayManager.Instance.AddPopulationLossText((long)currentSunStarCasualties, "{0} fried ({1}%)");
                currentSunStarCasualties = 0;
            }
            if (currentBlackHoleCasualties > 0)
            {
                GameplayManager.Instance.AddPopulationLossText((long)currentBlackHoleCasualties, "{0} spaghettified ({1}%)");
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
        if (invincible) return;


        SoundManager.Instance.PlayRandomHit(hitAudioClips, hitVolume);

        if (GameplayManager.Instance.ShieldAvailable())
        {
            StartCoroutine(IFramesShield(shieldRenderer));
        }


        // Calculate hit fraction:
        float maxHitPercent = 100f;
        float maxHitForce = 20f;
        float hitPercent = maxHitPercent * Mathf.Clamp01(collision.relativeVelocity.magnitude / maxHitForce);
        float hitFraction = hitPercent / 100f;

        long peopleDied = GameplayManager.Instance.TakeHit(hitFraction, 1000);

        // Shaking the screen in direction of the collision
        EffectsManager.Instance.Shake(collision.relativeVelocity);

        if (peopleDied > 0)
        {
            SoundManager.Instance.PlayScreams(screamsVolume);
            SoundManager.Instance.PlayRandomQuake(quakeAudioClips, quakeVolume);

            StartCoroutine(IFrames(rendr));

            peopleParticleSystem.Emit((int)Mathf.Log10(peopleDied));
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

    private IEnumerator IFrames(SpriteRenderer r)
    {
        invincible = true;
        float flashSpeed = 8f;
        float elapsed = 0f;

        while (elapsed < invincibilityDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * flashSpeed, 1f);

            // We lerp from the original color to the half of the red (so not so much red)
            r.color = Color.Lerp(originalColor, Color.Lerp(originalColor, Color.red, 0.5f), t);
            yield return null;
        }

        r.color = originalColor;
        invincible = false;
    }

    private IEnumerator IFramesShield(SpriteRenderer r)
    {
        invincible = true;
        float elapsed = 0f;
        Color originalColor = r.color;

        while (elapsed < invincibilityDurationSeconds)
        {
            elapsed += Time.deltaTime;

            Color color = r.color;
            color.a = Mathf.PingPong(Time.time * flashSpeed, 1f);

            r.color = color;
            yield return null;
        }

        r.color = originalColor;
        invincible = false;
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
        explosionAnimation.gameObject.SetActive(true);
    }

    public void DeathAnimationEnded()
    {
        Destroy(gameObject);
    }

}
