using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SwarmFollow))]
[RequireComponent(typeof(AudioSource))]
public class FlyingSaucerController : MonoBehaviour, IPlayerHitReceiver
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
    public FlyingSaucerState state = FlyingSaucerState.ACTIVE;

    [SerializeField]
    private float damageFactor = 100f;

    private bool playerInShootingRange = false;

    [SerializeField] private float damageFlashingDurationSeconds = 1f;
    private Color originalColor;
    private Coroutine flashingCoroutine;

    [SerializeField] private ParticleSystem smokeEmitter;

    [Header("Audio")]
    [SerializeField] private AudioSource laserAudioSource;
    [SerializeField] private AudioClip[] laserSoundClips;

    public event Action DamageTaken;

    private float accruedDamage = 0f;

    [SerializeField] private float maxSmokeEmission = 50f;

    private Rigidbody2D rb;

    /*
    [Header("Border Damage")]
    [SerializeField] private float borderDangerMargin = 2f;
    [SerializeField] private float borderDangerMultiplier = 100f;
    */

    void Awake()
    {
        swarmFollow = GetComponent<SwarmFollow>();
        originalColor = spriteRenderer.color;
        CurrentLife = maxLife;

        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        CurrentLife = maxLife;
        halfLife = maxLife * 0.5f;
    }

    void Update()
    {
        if (state == FlyingSaucerState.DEAD) return;

        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (CurrentLife < halfLife)
        {
            // UFO will bleed, which will make more of them fall and die.
            accruedDamage += Time.deltaTime;
            if (accruedDamage > 1f)
            {
                // This whole accruedDamage logic is so that we don't send damage event to UI every frame
                accruedDamage -= 1f;
                TakeHit(1f);// 1 HP per second
            }

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
            StartCoroutine(DeathSequence());
        }

        if (state != FlyingSaucerState.ACTIVE)
        {
            return;
        }


        /*
         * This would actually make the whole event much too easy :(
         *
        // Calculating damage due to being too close to the boundary
        float height = Camera.main.orthographicSize * 2f;

        // TODO This is stolen from the PlanetController, maybe refactor later?
        float deathHeightTop = Camera.main.transform.position.y + height / 2;
        float dangerHeightTop = deathHeightTop - borderDangerMargin;

        float deathHeightBottom = Camera.main.transform.position.y - height / 2;
        float dangerHeightBottom = deathHeightBottom + borderDangerMargin;

        float outOfBoundsDamagePerSecond = 0f;

        if (transform.position.y <= dangerHeightBottom)
        {
            float distance = Mathf.Abs(transform.position.y - dangerHeightBottom);
            outOfBoundsDamagePerSecond = distance / borderDangerMargin;
        }
        else if (transform.position.y >= dangerHeightTop)
        {
            float distance = Mathf.Abs(transform.position.y - dangerHeightTop);
            outOfBoundsDamagePerSecond = distance / borderDangerMargin;
        }

        TakeHit(outOfBoundsDamagePerSecond * borderDangerMultiplier * Time.deltaTime);
        */


        if (playerInShootingRange)
        {
            if (currentCooldown <= 0)
            {
                BulletController bullet = InstancePoolsManager.Instance.BulletControllerPool.Get();
                bullet.Init();

                bullet.transform.position = transform.position;

                // Adding spread when the UFO is dying
                Vector2 to = GameplayManager.Instance.Player.transform.position;
                if (CurrentLife <= halfLife)
                {
                    to += Random.insideUnitCircle * (5f * (CurrentLife / halfLife));
                }

                bullet.FromTo(transform.position, to);
                bullet.speed = bulletSpeed;

                AudioClip clip = laserSoundClips[Random.Range(0, laserSoundClips.Length)];
                laserAudioSource.pitch = Random.Range(1f, 2f);
                laserAudioSource.PlayOneShot(clip);

                currentCooldown = bulletCooldown;
            }
        }
    }

    public bool IsFullHealth()
    {
        return CurrentLife == maxLife;
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

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (state != FlyingSaucerState.ACTIVE)
        {
            return;
        }
        if (collider.CompareTag("Player"))
        {
            playerInShootingRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (state != FlyingSaucerState.ACTIVE)
        {
            return;
        }

        if (collider.CompareTag("Player"))
        {
            playerInShootingRange = false;
        }
    }

    IEnumerator HitStun()
    {
        state = FlyingSaucerState.STUNNED;
        yield return WAIT_ONE_SECOND;

        state = FlyingSaucerState.ACTIVE;
    }

    IEnumerator DeathSequence()
    {
        state = FlyingSaucerState.DEAD;

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

    public void PlayerHit(PlayerHitType type)
    {
        if (state != FlyingSaucerState.ACTIVE)
        {
            return;
        }

        PlanetController player = GameplayManager.Instance.Player;
        Vector2 relativeVelocity = rb.linearVelocity - player.GetVelocity();

        long damage = (long)(relativeVelocity.magnitude * IPlayerHitReceiver.CalculateHitFraction(type) * damageFactor);

        Debug.Log("Damage: " + gameObject.GetInstanceID() + " = " + damage);
        TakeHit(damage, true);
    }


    public void TakeHit(float hit, bool stun = false)
    {
        if (state == FlyingSaucerState.DEAD)
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

public enum FlyingSaucerState
{
    ACTIVE, STUNNED, DEAD
}
