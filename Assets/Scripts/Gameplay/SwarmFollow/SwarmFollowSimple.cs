using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwarmFollowSimple : MonoBehaviour
{
    [SerializeField] private Transform leader;
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Swarm Offset")]
    [SerializeField] private Vector2 offsetFromLeader = Vector2.zero;
    [SerializeField] private float offsetNoiseAmount = 0.4f;
    [SerializeField] private float offsetNoiseSpeed = 0.8f;

    private Vector2 noiseOffset;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        noiseOffset = Random.insideUnitCircle * 100f;
    }

    void FixedUpdate()
    {
        float nx = (Mathf.PerlinNoise(Time.time * offsetNoiseSpeed + noiseOffset.x, 0f) - 0.5f) * offsetNoiseAmount;
        float ny = (Mathf.PerlinNoise(0f, Time.time * offsetNoiseSpeed + noiseOffset.y) - 0.5f) * offsetNoiseAmount;

        Vector2 targetPosition = (Vector2)leader.position
            + offsetFromLeader
            + new Vector2(nx, ny);

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, followSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        Vector2 velocity = targetPosition - rb.position;
        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle - 90f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
