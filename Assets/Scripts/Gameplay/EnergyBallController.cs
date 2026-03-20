using UnityEngine;
using PotatoGameDev.Pool;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnergyBallController : PooledInstance
{
    private readonly float SHRINKING_DISTANCE = 2f;

    public EnergyType Type { get; set; }
    public Vector2? Target { get; set; }
    private Animator anim;
    private Vector3 initialScale;
    private float speed = 0.1f;
    private static readonly float initialSpeed = 0.1f;

    public int energyValue = 1;

    private CircleCollider2D col;

    void Awake()
    {
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;

        col = GetComponent<CircleCollider2D>();
    }

    void FixedUpdate()
    {
        if (Type == EnergyType.PipeEnergy && !GameplayManager.Instance.Player.Dead)
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

        if (Type == EnergyType.CollectEnergy && Target != null)
        {
            if (Target is Vector2 target)
            {
                transform.position = Vector2.Lerp(transform.position, target, speed);
                return;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !GameplayManager.Instance.Player.Dead)
        {
            if (Type == EnergyType.CollectEnergy)
            {
                GameplayManager.Instance.scoopedEnergy += energyValue;
            }
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
        Target = null;
        Type = EnergyType.CollectEnergy;
    }

    public bool CanPlace(Vector2 position, LayerMask blockingLayer)
    {
        return Physics2D.OverlapBox(position + col.offset, Vector2.one * col.radius, 0f, blockingLayer) == null;
    }
}

public enum EnergyType
{
    PipeEnergy, CollectEnergy
}
