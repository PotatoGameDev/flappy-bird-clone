using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FollowRigidbody : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (GameplayManager.Instance.Player.Dead) return;

        Vector2 newPos = rb.position;
        newPos.x += GameplayManager.Instance.Player.speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
}
