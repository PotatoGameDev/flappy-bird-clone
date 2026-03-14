using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    private readonly float RPM_PENALTY_THRESHOLD = 20f;

    private readonly WaitForSeconds EVERY_SECOND = new(1);

    public float speed = 5f;
    [SerializeField] private float speedIncrease = 0.1f;

    [SerializeField]
    private float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer rendr;

    [SerializeField]
    private float invincibilityDurationSeconds = 2f;
    [SerializeField]
    private float flashSpeed = 1f;
    private bool invincible = false;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();

        GameplayManager.Instance.Player = this;
    }

    void Start()
    {
        // Start updating rotational penalties
        StartCoroutine(UpdateRorationalPenalties());

    }

    void Update()
    {
        if (!alive)
        {
            rb.linearVelocity = Vector2.zero;

            return;
        }

        if (currentJumpForce > 0f)
        {
            rb.linearVelocity = new Vector2(speed, currentJumpForce);
            currentJumpForce = 0f;
        }
        else
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
    }

    private IEnumerator UpdateRorationalPenalties()
    {
        while (true)
        {
            float rpmAbs = Mathf.Abs(GetRpm());
            float penaltyRpm = 0f;

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

    // Life
    private bool alive = true;

    public bool Dead()
    {
        return !alive;
    }

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
        if (!alive) return;
        if (ctx.performed)
        {
            Death();
        }
    }

    // Collisions:

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!alive) return;
        if (invincible) return;

        StartCoroutine(IFrames());

        SoundManager.Instance.PlayScreams(screamsVolume);
        SoundManager.Instance.PlayRandomQuake(quakeAudioClips, quakeVolume);
        SoundManager.Instance.PlayRandomHit(hitAudioClips, hitVolume);

        GameplayManager.Instance.TakeHit(collision.relativeVelocity.magnitude);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (!alive) return;
        if (collider.gameObject.CompareTag("BoundaryBack"))
        {
            Death();
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

    public int GetRpm()
    {
        return (int)(rb.angularVelocity / 6f);
    }

    public void AddRpm(float rpm)
    {
        rb.angularVelocity += rpm * 6f;
    }

    private IEnumerator IFrames()
    {
        invincible = true;
        float elapsed = 0f;
        Color originalColor = rendr.color;

        while (elapsed < invincibilityDurationSeconds)
        {
            elapsed += Time.deltaTime;

            Color color = rendr.color;
            color.a = Mathf.PingPong(Time.time * flashSpeed, 1f);

            rendr.color = color;
            yield return null;
        }

        rendr.color = originalColor;
        invincible = false;
    }

    public void Death()
    {
        // Letting know other components that the player died
        alive = false;

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
