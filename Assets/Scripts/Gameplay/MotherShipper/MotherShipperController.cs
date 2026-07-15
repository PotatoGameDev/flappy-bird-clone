using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SwarmFollow))]
public class MotherShipperController : MonoBehaviour, IPlayerHitReceiver
{
    private static readonly WaitForSeconds WAIT_ONE_SECOND = new(1f);
    private static readonly WaitForSeconds WAIT_TENTH_OF_SECOND = new(0.1f);

    private SwarmFollow swarmFollow;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float bulletCooldown = 0.5f;
    [SerializeField] private float bulletSpeed = 15f;
    private float currentCooldown = 0f;

    [SerializeField] private long maxLife = 100L;

    private long halfLife;
    public long CurrentLife { get; private set; }
    private bool dead;

    [SerializeField]
    private float damageFactor = 100f;


    [SerializeField]
    private long cannonDamagePerSecond = 10L;


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
    [SerializeField] private LineRenderer plasmaBeamLineRenderer;
    [SerializeField] private float plasmaCannonRotationSpeed = 45f;

    [SerializeField] private float plasmaCannonRadiusMin;
    [SerializeField] private float plasmaCannonRadiusMax;
    private float plasmaCannonRadiusCurrent;

    [SerializeField] private float plasmaBeamTargettingWidthMin = 0.05f;
    [SerializeField] private float plasmaBeamTargettingWidthMax = 0.5f;
    private float plasmaBeamTargettingWidthCurrent;

    [SerializeField] private float plasmaBeamWidthMax = 1f;
    [SerializeField] private float plasmaBeamLengthMax = 30f;

    [SerializeField]
    private SwarmController[] swarmControllers;

    private Phase phase = Phase.Idle;
    private float cannonAngle;
    private Vector3 orbitOffset;


    private int waypointsTillAttack;

    [SerializeField] private float flyingSaucerDamage = 50f;

    [Header("Other")]
    [SerializeField] private EffectsManager effectsManager;

    [SerializeField] private float explosionCooldownSeconds = 0.1f;
    private float lastExplosionTime = -Mathf.Infinity;

    private long totalBeamKills = 0L;

    void Awake()
    {
        swarmFollow = GetComponent<SwarmFollow>();
        originalColor = spriteRenderer.color;
        CurrentLife = maxLife;
    }

    void Start()
    {
        CurrentLife = maxLife;
        halfLife = (long)(maxLife * 0.5f);

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

        // Initially we set the beam line to start and end in the ship 0,0
        plasmaBeamLineRenderer.SetPositions(
                new Vector3[] {
                    transform.position,
                    transform.position
                });

        GameplayManager.Instance.OnGatePassed += GatePassed;
    }

    void OnDestroy()
    {
        GameplayManager.Instance.OnGatePassed -= GatePassed;
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

        if (CurrentLife <= 0L)
        {
            plasmaCannon.SetActive(false);

            StartCoroutine(DeathSequence());
        }

        if (!Active)
        {
            // For this type of boss this is not needed.
            //return;
        }

        Transform playerTransform = GameplayManager.Instance.Player.transform;

        float rad = cannonAngle * Mathf.Deg2Rad;
        orbitOffset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad))
            * plasmaCannonRadiusCurrent;
        plasmaCannon.transform.position = transform.position
            + orbitOffset;
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

                SetBeam(
                        orbitOffset.normalized,
                        plasmaBeamLengthMax,
                        plasmaBeamTargettingWidthCurrent,
                        plasmaBeamTargettingWidthCurrent // used as opacity
                        );

                Vector3 toPlayer = playerTransform.position - transform.position;

                float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x)
                    * Mathf.Rad2Deg;

                if (Mathf.Approximately(targetAngle, cannonAngle))
                {
                    phase = Phase.StartFiring;

                    plasmaCannonAudioSource.PlayOneShot(
                            plasmaCannonFireSoundClip
                            );
                    plasmaCannonAudioSource.loop = false;
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
                    + orbitOffset;
                plasmaCannon.transform.up = -orbitOffset.normalized;


                break;
            case Phase.StartFiring:
                StartCoroutine(FiringCoroutine());
                phase = Phase.SignalFiring;
                break;
            case Phase.SignalFiring:
                // Nothing
                break;
            case Phase.Firing:
                // Nothing
                break;
            case Phase.Idle:
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

    void FixedUpdate()
    {
        if (phase == Phase.Firing)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    orbitOffset,
                    plasmaBeamLengthMax,
                    LayerMask.GetMask("Player", "Ufos")
                    );

            if (hit)
            {
                SetBeam(
                        orbitOffset.normalized,
                        hit.distance,
                        1f,
                        1f
                        );

                if (hit.collider.CompareTag("Player"))
                {
                    if (Time.time - lastExplosionTime >= explosionCooldownSeconds)
                    {
                        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
                        explosion.transform.position = hit.point;
                        explosion.Init();
                        lastExplosionTime = Time.time;

                        effectsManager.Shake();
                    }

                    long peopleDied = GameplayManager.Instance.TakeHit(
                            new(0L,
                                (long)(cannonDamagePerSecond * Time.deltaTime),
                                false,
                                false,
                                Vector2.zero
                                )
                            );


                    totalBeamKills += peopleDied;
                }
                else
                {
                    // Ufos:
                    FlyingSaucerController flyingSaucer = hit.collider.GetComponentInParent<FlyingSaucerController>();
                    flyingSaucer.TakeHit(
                            (long)(flyingSaucerDamage * Time.deltaTime)
                            );

                    if (Time.time - lastExplosionTime >= explosionCooldownSeconds)
                    {
                        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
                        explosion.transform.position = hit.point;
                        explosion.Init();
                        lastExplosionTime = Time.time;
                    }
                }
            }
            else
            {
                SetBeam(
                        orbitOffset.normalized,
                        plasmaBeamLengthMax,
                        1f,
                        1f
                        );

                if (totalBeamKills > 0L)
                {
                    GameplayManager.Instance.AddPopulationLossText(peopleDied: totalBeamKills);
                    totalBeamKills = 0L;
                }
            }

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
                SetBeam(
                        orbitOffset.normalized,
                        plasmaBeamLengthMax,
                        plasmaBeamTargettingWidthMin,
                        0f
                        );

                plasmaBeamLineRenderer.enabled = true;

                plasmaCannonAudioSource.PlayOneShot(plasmaCannonBootUpSoundClip);
                plasmaCannonAudioSource.loop = true;
            }
            else
            {
                waypointsTillAttack--;
            }
        }
    }

    IEnumerator FiringCoroutine()
    {
        // The beam at min scale to signal danger to the player:
        SetBeam(
                orbitOffset.normalized,
                plasmaBeamLengthMax,
                plasmaBeamTargettingWidthMin,
                1.0f
                );

        yield return WAIT_TENTH_OF_SECOND;

        // Hide the beam for a moment:
        plasmaBeamLineRenderer.enabled = false;

        yield return WAIT_TENTH_OF_SECOND;

        // The beam at full scale
        plasmaBeamLineRenderer.enabled = true;

        phase = Phase.Firing;

        SetBeam(
                orbitOffset.normalized,
                plasmaBeamLengthMax,
                1.0f,
                1.0f
                );

        effectsManager.StartSustainedShake(ShakeSource.PlasmaBeam, 2.0f);

        yield return WAIT_ONE_SECOND;
        yield return WAIT_ONE_SECOND;

        effectsManager.StopSustainedShake(ShakeSource.PlasmaBeam);
        plasmaBeamLineRenderer.enabled = false;

        phase = Phase.Idle;
    }

    public bool IsFullHealth()
    {
        return CurrentLife == maxLife;
    }

    public void SetBeam(
            Vector3 direction,
            float length,
            float width,
            float opacity)
    {
        // 1 is the end point of the line, we only have 2 points, start and end
        plasmaBeamLineRenderer.SetPosition(0, transform.position);
        plasmaBeamLineRenderer.SetPosition(1, transform.position + direction * length);
        plasmaBeamLineRenderer.startWidth = width;
        plasmaBeamLineRenderer.endWidth = width;

        Color plasmaColor = plasmaBeamLineRenderer.startColor;
        plasmaColor.a = opacity;
        plasmaBeamLineRenderer.startColor = plasmaColor;
        plasmaBeamLineRenderer.endColor = plasmaColor;
    }

    public void Repair(int energy)
    {
        CurrentLife = (long)Mathf.Clamp(CurrentLife + energy, 0L, maxLife);

        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
        }
        flashingCoroutine = StartCoroutine(HealthFlashing(false));
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

    public void PlayerHit(Damage damage)
    {
        if (!IsHittable())
        {
            return;
        }

        TakeHit(damage.enemy, true);
    }

    public bool IsHittable()
    {
        return !dead && Active;
    }

    public long GetLifeUnit()
    {
        return maxLife;
    }

    private void TakeHit(long hit, bool stun = false)
    {
        if (dead)
        {
            // Don't check for swarmFollow.Active,
            // because we want to do accrued damage even when not.
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

    private void GatePassed()
    {
        FlyingSaucerController flyingSaucer = InstancePoolsManager.Instance.
            FlyingSaucerControllerPool.Get();

        flyingSaucer.Init();

        flyingSaucer.transform.position = transform.position +
            Vector3.left * 2f;

        SwarmController swarmController = swarmControllers[
            Random.Range(0, swarmControllers.Length)
        ];

        swarmController.AddBoid(flyingSaucer.GetComponent<SwarmFollow>());
    }
}

enum Phase
{
    Idle,
    Targetting,
    StartFiring,
    SignalFiring,
    Firing
}
