using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SwarmFollow))]
[RequireComponent(typeof(SpriteRenderer))]
public class FlyingSaucerController : MonoBehaviour
{
    private static readonly WaitForSeconds DEATH_SEQUENCE_WAIT = new(1f);

    private Rigidbody2D rb;
    private SwarmFollow swarmFollow;
    private SpriteRenderer rendr;

    [SerializeField] private float bulletCooldown = 0.5f;
    [SerializeField] private float bulletSpeed = 15f;
    private float currentCooldown = 0f;

    [SerializeField] private float maxLife = 100f;
    private float currentLife;

    /*
    [Header("Border Damage")]
    [SerializeField] private float borderDangerMargin = 2f;
    [SerializeField] private float borderDangerMultiplier = 100f;
    */

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        swarmFollow = GetComponent<SwarmFollow>();
        rendr = GetComponent<SpriteRenderer>();
    }


    void Start()
    {
        currentLife = maxLife;
    }

    void Update()
    {
        if (!Alive()) return;

        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
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
        if (!collider.CompareTag("Player"))
        {
            return;
        }
        if (currentCooldown <= 0)
        {
            BulletController bullet = InstancePoolsManager.Instance.BulletControllerPool.Get();
            bullet.Init();

            bullet.transform.position = transform.position;
            bullet.FromTo(transform.position, collider.transform.position);
            bullet.speed = bulletSpeed;

            currentCooldown = bulletCooldown;
        }
    }

    IEnumerator DeathSequence()
    {
        rb.gravityScale = 1f;
        swarmFollow.Active = false;

        yield return DEATH_SEQUENCE_WAIT;

        rendr.enabled = false;

        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();
        explosion.Init();
        explosion.transform.SetParent(transform);
        explosion.transform.localScale = Vector2.one * 0.5f;
        explosion.transform.localPosition = Vector2.zero;

        explosion.OnFinished.AddListener(DeathAnimationEnded);
    }

    public void DeathAnimationEnded()
    {
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
        if (!Alive())
        {
            return;
        }

        currentLife -= hit;

        // Post
        if (currentLife <= 0f)
        {
            StartCoroutine(DeathSequence());
        }
        else if (currentLife < maxLife * 0.5f)
        {
            // start smoke
        }
    }

    private bool Alive()
    {
        return currentLife > 0f;
    }
}
