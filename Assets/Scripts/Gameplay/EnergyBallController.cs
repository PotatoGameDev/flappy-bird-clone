using UnityEngine;
using PotatoGameDev.Pool;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnergyBallController : PooledInstance
{
    private readonly float SHRINKING_DISTANCE = 2f;
    private readonly WaitForSeconds TIMEOUT = new(5);

    public EnergyType Type { get; set; }
    private Vector2? target;
    private Transform targetTransform;
    private Animator anim;
    private Vector3 initialScale;
    private float speed = 0.1f;
    private static readonly float initialSpeed = 0.1f;

    public int energyValue = 1;

    public CircleCollider2D Collider { get; set; }
    private SpriteRenderer rendr;

    private Coroutine timeout;

    void Awake()
    {
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;


        Collider = GetComponent<CircleCollider2D>();
        rendr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (Type == EnergyType.PipeEnergy && !GameplayManager.Instance.Player.Dead)
        {
            transform.position = Vector2.Lerp(transform.position, GameplayManager.Instance.Player.transform.position, speed);
            speed += 0.1f * Time.fixedDeltaTime; // This makes the energy ball always catch up to the planet, no matter of it's speed.
                                                 // The energy ball will keep accelerating, eventually going faster than the planet.

            float distToPlayer = Vector2.Distance(transform.position, GameplayManager.Instance.Player.transform.position);
            if (distToPlayer < SHRINKING_DISTANCE)
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, distToPlayer / SHRINKING_DISTANCE);
            }
            return;
        }

        if (Type == EnergyType.CollectEnergy && target != null)
        {
            if (target is Vector2 targetValue)
            {
                speed += 0.1f * Time.fixedDeltaTime; // This makes the energy ball always catch up to the planet, no matter of it's speed.
                                                     // The energy ball will keep accelerating, eventually going faster than the planet.
                transform.position = Vector2.Lerp(transform.position, targetValue, speed);
                return;
            }
        }

        if (Type == EnergyType.CollectEnergy && targetTransform != null)
        {
            speed += 0.1f * Time.fixedDeltaTime; // This makes the energy ball always catch up to the planet, no matter of it's speed.
                                                 // The energy ball will keep accelerating, eventually going faster than the planet.
            transform.position = Vector2.Lerp(transform.position, targetTransform.position, speed);

            return;
        }
    }

    public void SetTargetVector(Vector3 value)
    {
        target = value;
        targetTransform = null;
    }

    public void SetTargetTransform(Transform value)
    {
        targetTransform = value;
        target = null;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            if (collider.transform == targetTransform)
            {
                FlyingSaucerController flyingSaucerController = targetTransform.GetComponent<FlyingSaucerController>();
                flyingSaucerController.Repair(energyValue);

                Release();
                return;
            }
        }

        if (collider.CompareTag("Player") && !GameplayManager.Instance.Player.Dead)
        {
            if (Type == EnergyType.CollectEnergy)
            {
                GameplayManager.Instance.AddScoopedEnergy(energyValue);
            }
            SoundManager.Instance.PlayCollect(GetPitch());
            Release();
            return;
        }
    }

    private float GetPitch()
    {
        return energyValue switch
        {
            1000 => 3f,
            100 => 2f,
            10 => 1.5f,
            1 => 1f,
            _ => 1f,
        };
    }

    public new void Release()
    {
        StopCoroutine(timeout);
        base.Release();
    }

    public new void Init()
    {
        // Start the looped animation from random frame:
        speed = initialSpeed;
        transform.localScale = initialScale;
        anim.Play(0, 0, Random.value);
        Type = EnergyType.CollectEnergy;
        target = null;
        targetTransform = null;

        timeout = StartCoroutine(Timeout());
    }

    IEnumerator Timeout()
    {
        yield return TIMEOUT;
        Release();
    }


    public void SetColor(Color color, int value)
    {
        rendr.color = color;
        energyValue = value;
    }
}

public enum EnergyType
{
    PipeEnergy, CollectEnergy
}
