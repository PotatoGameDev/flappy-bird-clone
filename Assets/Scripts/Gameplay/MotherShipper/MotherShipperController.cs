using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class MotherShipperController : MonoBehaviour
{
    private static readonly WaitForSeconds WAIT_ONE_SECOND = new(1f);

    private SwarmFollow swarmFollow;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float bulletCooldown = 0.5f;
    [SerializeField] private float bulletSpeed = 15f;
    private float currentCooldown = 0f;

    [SerializeField] private float maxLife = 100f;

    private float halfLife;
    public float CurrentLife { get; private set; }
    private bool dead;

    public bool Active
    {
        get => swarmFollow.Active;
        set => swarmFollow.Active = value;
    }

    [SerializeField] private float damageFlashingDurationSeconds = 1f;
    private Color originalColor;
    private Coroutine flashingCoroutine;

    [SerializeField] private ParticleSystem smokeEmitter;

    [Header("Audio")]
    [SerializeField] private AudioSource plasmaCannonAudioSource;
    [SerializeField] private AudioClip plasmaCannonBootUpSoundClip;
    [SerializeField] private AudioClip plasmaCannonFireSoundClip;

    public event Action DamageTaken;

    [SerializeField] private float maxSmokeEmission = 50f;

    [Header("Plasma Cannon")]
    [SerializeField] private GameObject plasmaCannon;
    [SerializeField] private SpriteRenderer plasmaBeamSprite;
    [SerializeField] private float plasmaCannonRotationSpeed = 45f;

    [SerializeField] private float plasmaCannonRadiusMin;
    [SerializeField] private float plasmaCannonRadiusMax;
    private float plasmaCannonRadiusCurrent;

    [SerializeField] private float plasmaBeamTargettingWidthMin = 0.05f;
    [SerializeField] private float plasmaBeamTargettingWidthMax = 0.5f;
    private float plasmaBeamTargettingWidthCurrent;

    private Phase phase = Phase.Idle;
    private float cannonAngle;
    private Vector2 orbitOffset;


    private int waypointsTillAttack;

    [Header("Other")]
    [SerializeField] private EffectsManager effectsManager;

    void Awake()
    {
        swarmFollow = GetComponent<SwarmFollow>();
        originalColor = spriteRenderer.color;
        CurrentLife = maxLife;
    }

    void Start()
    {
        CurrentLife = maxLife;
        halfLife = maxLife * 0.5f;

        Vector3 toCannon = plasmaCannon.transform.position - transform.position;
        if (Mathf.Approximately(toCannon.magnitude, 0f))
        {
            Vector3 pos = plasmaCannon.transform.position;
            pos.x += plasmaCannonRadiusCurrent;
            plasmaCannon.transform.position = pos;
        }

        phase = Phase.Idle;

        cannonAngle = Mathf.Atan2(toCannon.y, toCannon.x) * Mathf.Rad2Deg;

        plasmaCannonRadiusCurrent = plasmaCannonRadiusMin;
        plasmaBeamTargettingWidthCurrent = plasmaBeamTargettingWidthMin;

        waypointsTillAttack = 5; // Initially we attack on the 5th waypoint
    }

    void Update()
    {
        if (dead) return;

        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (CurrentLife < halfLife)
        {
            if (!smokeEmitter.isPlaying)
            {
                smokeEmitter.Play();
            }
            ParticleSystem.EmissionModule emission = smokeEmitter.emission;
            emission.rateOverTime = maxSmokeEmission * CurrentLife / halfLife;
        }
        else
        {
            smokeEmitter.Stop();
        }

        if (CurrentLife <= 0f)
        {
            plasmaCannon.SetActive(false);

            StartCoroutine(DeathSequence());
        }

        if (!Active)
        {
            //return;
        }

        Transform playerTransform = GameplayManager.Instance.Player.transform;

        float rad = cannonAngle * Mathf.Deg2Rad;
        orbitOffset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad))
            * plasmaCannonRadiusCurrent;
        plasmaCannon.transform.position = transform.position
            + (Vector3)orbitOffset;
        plasmaCannon.transform.up = -orbitOffset.normalized;

        switch (phase)
        {
            case Phase.Targetting:
                plasmaCannonRadiusCurrent = Mathf.Lerp(
                        plasmaCannonRadiusCurrent,
                        plasmaCannonRadiusMax,
                        2f * Time.deltaTime
                        );

                plasmaBeamTargettingWidthCurrent = Mathf.Lerp(
                        plasmaBeamTargettingWidthCurrent,
                        plasmaBeamTargettingWidthMax,
                        2f * Time.deltaTime
                        );

                SetBeamWidth(plasmaBeamTargettingWidthCurrent);

                Vector3 toPlayer = playerTransform.position - transform.position;

                float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x)
                    * Mathf.Rad2Deg;

                if (Mathf.Approximately(targetAngle, cannonAngle))
                {
                    phase = Phase.StartFiring;
                    break;
                }

                cannonAngle = Mathf.MoveTowardsAngle(
                        cannonAngle,
                        targetAngle,
                        plasmaCannonRotationSpeed * Time.deltaTime
                        );

                rad = cannonAngle * Mathf.Deg2Rad;
                orbitOffset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad))
                    * plasmaCannonRadiusCurrent;
                plasmaCannon.transform.position = transform.position
                    + (Vector3)orbitOffset;
                plasmaCannon.transform.up = -orbitOffset.normalized;


                break;
            case Phase.StartFiring:
                StartCoroutine(FiringCoroutine());
                phase = Phase.Firing;
                break;
            case Phase.Firing:
                // Nothing
                break;
            case Phase.Idle:
                // TODO: Some better condition for this trigger:
                plasmaCannonRadiusCurrent = Mathf.Lerp(
                        plasmaCannonRadiusCurrent,
                        plasmaCannonRadiusMin,
                        2f * Time.deltaTime
                        );

                break;
            default:
                throw new Exception("Wrong state");
        }
    }

    public void WaypointReached(int _)
    {
        if (phase == Phase.Idle)
        {
            if (waypointsTillAttack == 0)
            {
                cannonAngle = 0f; // Starts targetting on the oposite of the player
                phase = Phase.Targetting;
                waypointsTillAttack = Random.Range(3, 7);
                SetBeamWidth(plasmaBeamTargettingWidthMin);

                plasmaBeamSprite.enabled = true;
            }
            else
            {
                waypointsTillAttack--;
            }
        }
    }

    IEnumerator FiringCoroutine()
    {
        // The beam at full scale
        SetBeamWidth(1.0f);

        effectsManager.StartSustainedShake(ShakeSource.PlasmaBeam, 2.0f);

        yield return WAIT_ONE_SECOND;
        yield return WAIT_ONE_SECOND;

        effectsManager.StopSustainedShake(ShakeSource.PlasmaBeam);
        plasmaBeamSprite.enabled = false;
        phase = Phase.Idle;
    }

    public bool IsFullHealth()
    {
        return CurrentLife == maxLife;
    }

    public void SetBeamWidth(float width)
    {
        Vector3 beamScale = plasmaBeamSprite.transform.localScale;
        beamScale.x = width;
        plasmaBeamSprite.transform.localScale = beamScale;

        Color plasmaColor = plasmaBeamSprite.color;
        plasmaColor.a = width;
        plasmaBeamSprite.color = plasmaColor;
    }

    public void Repair(int energy)
    {
        CurrentLife = Mathf.Clamp(CurrentLife + energy, 0f, maxLife);

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }
        flashingCoroutine = StartCoroutine(HealthFlashing(false));
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (dead || !Active)
        {
            return;
        }

        if (collider.CompareTag("Player"))
        {
            if (currentCooldown <= 0)
            {
                BulletController bullet = InstancePoolsManager.Instance.BulletControllerPool.Get();
                bullet.Init();

                bullet.transform.position = transform.position;

                // Adding spread when the UFO is dying
                Vector2 to = collider.transform.position;
                if (CurrentLife <= halfLife)
                {
                    to += Random.insideUnitCircle * (5f * (CurrentLife / halfLife));
                }

                bullet.FromTo(transform.position, to);
                bullet.speed = bulletSpeed;

                AudioClip clip = plasmaCannonFireSoundClip;
                plasmaCannonAudioSource.PlayOneShot(clip);

                currentCooldown = bulletCooldown;
            }
        }
    }

    IEnumerator HitStun()
    {
        Active = false;
        yield return WAIT_ONE_SECOND;

        Active = true;
    }

    IEnumerator DeathSequence()
    {
        dead = true;
        Active = false;

        yield return WAIT_ONE_SECOND;

        spriteRenderer.enabled = false;

        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
        explosion.Init();
        explosion.OnFinished.AddListener(DeathAnimationEnded);

        explosion.transform.SetParent(transform);
        explosion.transform.localScale = Vector2.one * 0.5f;
        explosion.transform.localPosition = Vector2.zero;

        SoundManager.Instance.PlayExplosion();
    }

    public void DeathAnimationEnded()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead || !Active)
        {
            return;
        }
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        TakeHit(5 * collision.relativeVelocity.magnitude, true);
    }


    private void TakeHit(float hit, bool stun = false)
    {
        if (dead)
        {
            // Don't check for swarmFollow.Active, because we want to do accrued damage even when not.
            return;
        }

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }
        flashingCoroutine = StartCoroutine(HealthFlashing(true));

        CurrentLife -= hit;

        if (stun)
        {
            StartCoroutine(HitStun());
        }

        DamageTaken?.Invoke();
    }

    private IEnumerator HealthFlashing(bool damage)
    {
        float flashSpeed = 8f;
        float elapsed = 0f;

        while (elapsed < damageFlashingDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * flashSpeed, 1f);

            // We lerp from the original color to the half of the red (so not so much red) (or green)
            if (damage)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.red, t);
            }
            else
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.green, t);
            }
            yield return null;
        }

        spriteRenderer.color = originalColor;
        flashingCoroutine = null;
    }

}

enum Phase
{
    Idle, Targetting, StartFiring, Firing
}
