using UnityEngine;
using PotatoGameDev.Pool;

[RequireComponent(typeof(Animator))]
public class EnergyBallController : PooledInstance
{
    private readonly float SHRINKING_DISTANCE = 2f;

    public bool FollowPlayer { get; set; }

    private Animator anim;
    private Vector3 origScale;
    private float speed = 0.1f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        origScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (!FollowPlayer || GameplayManager.Instance.Player.Dead())
        {
            return;
        }
        transform.position = Vector2.Lerp(transform.position, GameplayManager.Instance.Player.transform.position, speed);
        speed += 0.1f * Time.deltaTime; // This makes the energy ball always catch up to the planet, no matter of it's speed.
                                        // The energy ball will keep accelerating, eventually going faster than the planet.

        float distToPlayer = Vector2.Distance(transform.position, GameplayManager.Instance.Player.transform.position);
        if (distToPlayer < SHRINKING_DISTANCE)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, origScale, distToPlayer / SHRINKING_DISTANCE);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Release();
        }
    }

    public new void Init()
    {
        // Start the looped animation from random frame:
        anim.Play(0, 0, Random.value);
    }
}
