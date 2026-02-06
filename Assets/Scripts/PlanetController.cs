using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    public float speed = 10f;
    public float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (currentJumpForce > 0f)
        {
            rb.linearVelocity = new Vector2(speed, currentJumpForce);
            currentJumpForce = 0f;
        }
        else
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            currentJumpForce += 10f;
        }
    }

}
