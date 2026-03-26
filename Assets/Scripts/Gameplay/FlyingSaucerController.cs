using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SwarmFollow))]
[RequireComponent(typeof(AudioSource))]
public class FlyingSaucerController : MonoBehaviour
{
    private static readonly WaitForSeconds DEATH_SEQUENCE_WAIT = new(1f);

    private Rigidbody2D rb;
    private SwarmFollow swarmFollow;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float bulletCooldown = 0.5f;
    [SerializeField] private float bulletSpeed = 15f;
    private float currentCooldown = 0f;

    [SerializeField] private float maxLife = 100f;
    private float currentLife;
    private bool dead;

    [SerializeField] private float damageFlashingDurationSeconds = 1f;
    private Color originalColor;
    private Coroutine flashingCoroutine;


    [SerializeField] private ParticleSystem smokeEmitter;

    private AudioSource audioSource;
    [SerializeField] private AudioClip[] laserSoundClips;

    /*
    [Header("Border Damage")]
    [SerializeField] private float borderDangerMargin = 2f;
    [SerializeField] private float borderDangerMultiplier = 100f;
    */

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        swarmFollow = GetComponent<SwarmFollow>();
        audioSource = GetComponent<AudioSource>();

        originalColor = spriteRenderer.color;
    }


    void Start()
    {
        currentLife = maxLife;
    }

    void Update()
    {
        if (dead) return;

        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (currentLife < 0.5f * maxLife)
        {
            // UFO will bleed, which will make more of them fall and die.
            currentLife -= Time.deltaTime; // 1 HP per second
        }

        // Post
        if (currentLife <= 0f)
        {
            StartCoroutine(DeathSequence());
        }
        else if (currentLife < maxLife * 0.5f)
        {
            smokeEmitter.Play();
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

    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (currentCooldown <= 0)
            {
                BulletController bullet = InstancePoolsManager.Instance.BulletControllerPool.Get();
                bullet.Init();

                bullet.transform.position = transform.position;
                bullet.FromTo(transform.position, collider.transform.position);
                bullet.speed = bulletSpeed;

                AudioClip clip = laserSoundClips[Random.Range(0, laserSoundClips.Length)];
                audioSource.pitch = Random.Range(1f, 2f);
                audioSource.PlayOneShot(clip);

                currentCooldown = bulletCooldown;
            }
        }
    }

    IEnumerator DeathSequence()
    {
        dead = true;
        rb.gravityScale = 1f;
        swarmFollow.Active = false;

        yield return DEATH_SEQUENCE_WAIT;

        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
        explosion.Init();
        explosion.transform.SetParent(transform);
        explosion.transform.localScale = Vector2.one * 0.5f;
        explosion.transform.localPosition = Vector2.zero;

        explosion.OnFinished.AddListener(DeathAnimationEnded);
    }

    public void DeathAnimationEnded()
    {
        spriteRenderer.enabled = false;
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        TakeHit(5 * collision.relativeVelocity.magnitude);
    }


    private void TakeHit(float hit)
    {
        if (dead)
        {
            return;
        }

        if (flashingCoroutine == null)
        {
            flashingCoroutine = StartCoroutine(DamageFlashing());
        }

        currentLife -= hit;

    }

    private IEnumerator DamageFlashing()
    {
        float flashSpeed = 8f;
        float elapsed = 0f;

        while (elapsed < damageFlashingDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * flashSpeed, 1f);

            // We lerp from the original color to the half of the red (so not so much red)
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, t);
            yield return null;
        }

        spriteRenderer.color = originalColor;
        flashingCoroutine = null;
    }

}
