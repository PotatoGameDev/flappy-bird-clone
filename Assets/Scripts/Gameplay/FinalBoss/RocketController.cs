using UnityEngine;
using PotatoGameDev.Pool;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class RocketController : PooledInstance
{
    public enum RocketType
    {
        Normal, Tiny
    }

    [SerializeField] internal RocketType type = RocketType.Normal;

    private enum State
    {
        ChasingPlayer,
        ChasingBoss,
    }

    private State state = State.ChasingPlayer;

    private static readonly WaitForSeconds TIMEOUT = new(3f);
    [SerializeField] private float movementForce = 100.0f;
    [SerializeField] private float movementForceBoss = 200.0f;

    [SerializeField]
    private float turnSpeedDegreesPerSecond = 90.0f;
    [SerializeField]
    private float maxSpeed = 3f;

    [SerializeField]
    private float initialBoostForce = 300f;

    [SerializeField] private float avoidRayDistance = 10f;
    [SerializeField] private float avoidAngleOffset = 45f;
    private int avoidBossSide = 0;

    [SerializeField] private float bounceBoostForce = 100f;

    private Rigidbody2D rb;

    public Vector3 InitialVelocity { get; set; }

    public FinalBossController Boss { get; set; }


    public new void Init()
    {
        base.Init();

        state = State.ChasingPlayer;

        rb.linearVelocity = InitialVelocity;
        rb.angularVelocity = 0f;
        rb.SetRotation(0f);
        rb.AddForce(Vector3.right * initialBoostForce, ForceMode2D.Impulse);

        StartCoroutine(Timeout());
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void PlayerHit(bool bounce)
    {
        if (bounce)
        {
            state = State.ChasingBoss;

            Vector2 direction = (Vector2)Boss.transform.position - rb.position;
            direction.Normalize();
            rb.AddForce(direction * bounceBoostForce, ForceMode2D.Impulse);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rb.rotation = angle;
        }
        else
        {
            Explode();
        }
    }

    void FixedUpdate()
    {

        if (state == State.ChasingPlayer)
        {
            var player = GameplayManager.Instance.Player;

            // Acceleration and speed control
            float rad = rb.rotation * Mathf.Deg2Rad;
            Vector2 forward = new(Mathf.Cos(rad), Mathf.Sin(rad));

            rb.AddForce(transform.right * movementForce, ForceMode2D.Force);

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity,
                        Mathf.Lerp(rb.linearVelocity.magnitude, maxSpeed, 0.1f)
                        );
            }

            // Angle, facing the player, so homing missile of sorts:
            Vector2 direction = (Vector2)player.transform.position - rb.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Avoid the boss
            RaycastHit2D hit = Physics2D.Raycast(rb.position, forward, avoidRayDistance, LayerMask.GetMask("Boss"));

            if (hit.collider != null)
            {
                if (avoidBossSide == 0)
                {
                    Vector2 toBoss = hit.point - rb.position;
                    avoidBossSide = Cross2D(forward, toBoss) >= 0f ? 1 : -1;
                }

                targetAngle -= avoidBossSide * avoidAngleOffset;
            }
            else
            {
                avoidBossSide = 0;
            }

            var newAngle = Mathf.MoveTowardsAngle(
                    rb.rotation,
                    targetAngle,
                    turnSpeedDegreesPerSecond * Time.fixedDeltaTime
                    );
            rb.MoveRotation(newAngle);
        }
        else if (state == State.ChasingBoss)
        {
            Vector2 direction = (Vector2)Boss.transform.position - rb.position;
            direction.Normalize();
            rb.AddForce(direction * movementForceBoss, ForceMode2D.Force);
        }
    }

    private static float Cross2D(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

    public void Explode()
    {
        ExplosionController explosion = InstancePoolsManager.Instance.ExplosionControllerPool.Get();

        explosion.transform.position = transform.position;

        Release();
    }

    IEnumerator Timeout()
    {
        yield return TIMEOUT;
        Explode();
    }

    public bool IsChasingBoss()
    {
        return state == State.ChasingBoss;
    }
}
