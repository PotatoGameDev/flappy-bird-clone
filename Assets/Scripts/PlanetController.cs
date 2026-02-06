using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlanetController : MonoBehaviour
{
    public float speed = 10f;
    public float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private Renderer rendr;
    private bool wasShown;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<Renderer>();

        GameManager.Instance.Player = gameObject;
    }

    void Update()
    {
        if (rendr.isVisible)
        {
            wasShown = true;
        }
        else
        {
            if (wasShown)
            {
                // Game over:
                GameManager.Instance.GameOver();
            }
        }
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
