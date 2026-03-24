using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwarmFollow : MonoBehaviour
{
    private Rigidbody2D rb;

    public Vector2 LinearVelocity => rb.linearVelocity;
    public Vector2 Position => rb.position;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public float Distance(SwarmFollow other)
    {
        return Vector2.Distance(rb.position, other.rb.position);
    }

    public void MovePosition(Vector2 newPosition)
    {
        rb.MovePosition(newPosition);
    }

    public void MovePositionDirect(Vector2 newPosition)
    {
        transform.position = newPosition;
    }


}
