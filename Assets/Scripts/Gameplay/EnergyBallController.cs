using UnityEngine;
using PotatoGameDev.Pool;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnergyBallController : PooledInstance
{
    private readonly float SHRINKING_DISTANCE = 2f;

    public bool FollowPlayer { get; set; }
    public bool FollowTarget { get; set; }
    public Vector2 Target { get; set; }
    private Animator anim;
    private Vector3 initialScale;
    private float speed = 0.1f;
    private static readonly float initialSpeed = 0.1f;

    private CircleCollider2D col;

    void Awake()
    {
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;

        col = GetComponent<CircleCollider2D>();
    }

    void FixedUpdate()
    {
        Debug.Assert(!FollowPlayer || !FollowTarget, "Cannot follow both player and target, check the logic");

        if (FollowPlayer && !GameplayManager.Instance.Player.Dead)
        {
            transform.position = Vector2.Lerp(transform.position, GameplayManager.Instance.Player.transform.position, speed);
            speed += 0.1f * Time.deltaTime; // This makes the energy ball always catch up to the planet, no matter of it's speed.
                                            // The energy ball will keep accelerating, eventually going faster than the planet.

            float distToPlayer = Vector2.Distance(transform.position, GameplayManager.Instance.Player.transform.position);
            if (distToPlayer < SHRINKING_DISTANCE)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, distToPlayer / SHRINKING_DISTANCE);
            }
            return;
        }

        if (FollowTarget)
        {
            transform.position = Vector2.Lerp(transform.position, Target, speed);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !GameplayManager.Instance.Player.Dead)
        {
            SoundManager.Instance.PlayCollect();
            Release();
        }
    }

    public new void Init()
    {
        // Start the looped animation from random frame:
        speed = initialSpeed;
        transform.localScale = initialScale;
        anim.Play(0, 0, Random.value);
        FollowPlayer = false;
        FollowTarget = false;
    }

    public bool CanPlace(Vector2 position, LayerMask blockingLayer)
    {
        return Physics2D.OverlapBox(position + col.offset, Vector2.one * col.radius, 0f, blockingLayer) == null;
    }
}
