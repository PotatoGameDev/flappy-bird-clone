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

    private float rotationDegreesPerSecond;
    private float wobbleAmplitude;
    private float wobbleSpeed;
    public Vector3 basePosition;

    void Awake()
    {
        anim = GetComponent<Animator>();
        initialScale = transform.localScale;

        Collider = GetComponent<CircleCollider2D>();
        rendr = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        // Rotating around:
        transform.Rotate(0f, 0f, rotationDegreesPerSecond * Time.fixedDeltaTime);

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
                basePosition = Vector2.Lerp(basePosition, targetValue, speed);

                ApplyWobble();

                if (Vector3.Distance(transform.position, targetValue) < 0.001f)
                {
                    target = null;
                }
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

        // Here we are already stationary and at target position:
        ApplyWobble();
    }

    private void ApplyWobble()
    {
        // Wobbling:
        float x = Mathf.PerlinNoise(Time.time * wobbleSpeed, 0f) - 0.5f;
        float y = Mathf.PerlinNoise(0f, Time.time * wobbleSpeed) - 0.5f;

        Vector3 offset = new Vector3(x, y, 0f) * wobbleAmplitude;
        transform.position = basePosition + offset;
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

    public void Init(Vector3 basePos)
    {
        Init();
        basePosition = basePos;
        transform.position = basePosition;
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
        rotationDegreesPerSecond = Random.Range(-90f, 90f);
        wobbleAmplitude = Random.Range(0f, 2f);
        wobbleSpeed = Random.Range(0f, 2f);

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
