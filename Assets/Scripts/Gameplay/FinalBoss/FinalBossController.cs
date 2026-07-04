using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    private readonly WaitForSeconds TINY_ROCKET_INTERVAL = new(0.1f);

    [SerializeField] private long life = 1_000_000_000L;
    [SerializeField] private long maxLife = 1_000_000_000L;

    // How much life has to go for every shield segment to fall off
    private const float lifeStepFraction = 0.1f;

    [SerializeField] private float delaySeconds = 0.1f;
    public float GetDelaySeconds() => delaySeconds;

    [SerializeField] private GameObject[] shieldChunks;

    [SerializeField] private float chunkFallOffForce = 10;

    public Transform SpriteHolder { get; private set; }

    public Vector3 PlayerOffset { get; private set; }

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Rigidbody2D playerRb;

    private int previousGateCount = 0;

    private struct PlayerSample
    {
        public Vector3 position;
        public float rotation;
        public bool gatePassed;
    }
    private readonly Queue<PlayerSample> history = new();

    private int delaySteps;

    private Vector2 previousPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.None;
    }

    void Start()
    {
        PlanetController player = GameplayManager.Instance.Player;
        playerTransform = player.transform;
        playerRb = player.GetComponent<Rigidbody2D>();

        delaySteps = Mathf.Max(0, Mathf.RoundToInt(delaySeconds / Time.fixedDeltaTime));
        float effectiveDelay = delaySteps * Time.fixedDeltaTime;

        float aheadDistance = player.speed * effectiveDelay;
        PlayerOffset = new Vector3(2f * aheadDistance, 0f, 0f);

        Vector3 playerPos = playerTransform.position;
        float startRotation = playerRb.rotation;
        for (int i = delaySteps; i >= 1; i--)
        {
            history.Enqueue(new PlayerSample
            {
                position = playerPos - new Vector3(player.speed * i * Time.fixedDeltaTime, 0f, 0f),
                rotation = startRotation,
                gatePassed = false,
            });
        }

        rb.position = playerPos + new Vector3(aheadDistance, 0f, 0f);
        rb.rotation = startRotation;
        previousPosition = rb.position;

        // Life
        life = maxLife;

        UpdateShield();

        GameplayManager.Instance.SetBossHealth(life, maxLife);

        // Selecting the sprite
        SpriteHolder = transform.Find("Sprite");

        string selectedPlanetName = "planet0" + GameManager.Instance.PlanetType.ToString();

        for (int i = 0; i < SpriteHolder.childCount; i++)
        {
            Transform child = SpriteHolder.GetChild(i);
            child.gameObject.SetActive(child.name == selectedPlanetName);
        }

    }

    private IEnumerator FireTinyRockets(int count)
    {
        for (int i = 0; i <= count; i++)
        {
            ShootRocket(RocketController.RocketType.Tiny);
            yield return TINY_ROCKET_INTERVAL;
        }
    }

    public void PlayerHit(bool parried)
    {
        // If the player hits the boss, it gives the boss full bar of damage

        float parryMultiplier = parried ? 0.1f : 1f;
        GetDamage((long)(maxLife * lifeStepFraction * parryMultiplier));
    }

    private void GetDamage(long damage)
    {
        if (damage > life)
        {
            damage = life;
        }

        life -= damage;

        GameplayManager.Instance.SetBossHealth(life, maxLife);

        UpdateShield();
    }

    private void UpdateShield()
    {
        float ratio = (float)life / maxLife;
        int shieldLevel = (int)(ratio * shieldChunks.Length);
        if (shieldLevel < 0 || shieldLevel >= shieldChunks.Length)
        {
            return;
        }

        for (int i = shieldLevel; i < shieldChunks.Length; i++)
        {
            GameObject chunk = shieldChunks[i];
            Rigidbody2D chunkRb = chunk.GetComponent<Rigidbody2D>();
            chunkRb.simulated = true;
            chunkRb.AddForce((rb.position - chunkRb.position) * chunkFallOffForce, ForceMode2D.Impulse);
        }
    }

    private void ShootRocket(RocketController.RocketType type = RocketController.RocketType.Normal)
    {
        RocketController rocket = type == RocketController.RocketType.Normal
            ? InstancePoolsManager.Instance.RocketControllerPool.Get()
            : InstancePoolsManager.Instance.TinyRocketControllerPool.Get();

        Vector2 velocity = rb.position - previousPosition;
        velocity.y *= -20f;

        if (type == RocketController.RocketType.Normal)
        {
            rocket.transform.position = transform.position + (Vector3.right * 1.2f);
        }
        else if (type == RocketController.RocketType.Tiny)
        {
            rocket.transform.position = transform.position - (Vector3.right * 1.2f);
            rocket.transform.Rotate(0, 0, 180);
        }

        rocket.Boss = this;
        rocket.InitialVelocity = velocity;
        rocket.Init();
    }

    void FixedUpdate()
    {
        bool passedGate = GameplayManager.Instance.GateCount > previousGateCount;
        history.Enqueue(new PlayerSample
        {
            position = playerTransform.position,
            rotation = playerRb.rotation,
            gatePassed = passedGate,
        });
        previousGateCount = GameplayManager.Instance.GateCount;

        while (history.Count > delaySteps + 1)
        {
            history.Dequeue();
        }

        PlayerSample sample = history.Peek();
        Vector3 targetPos = sample.position + PlayerOffset;

        rb.MovePosition(targetPos);
        rb.MoveRotation(sample.rotation);

        if (sample.gatePassed)
        {
            int val = Random.Range(0, 2);
            if (val == 0)
            {
                ShootRocket();
            }
            else
            {
                StartCoroutine(FireTinyRockets(
                            GameplayManager.Instance.GateCount
                            ));
            }
        }

        previousPosition = rb.position;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rocket"))
        {
            RocketController rocket = collision.gameObject.GetComponent<RocketController>();

            if (rocket.IsChasingBoss())
            {
                rocket.Explode();

                if (rocket.type == RocketController.RocketType.Normal)
                {
                    GetDamage((long)(maxLife * lifeStepFraction));
                }
            }
        }
    }
}
