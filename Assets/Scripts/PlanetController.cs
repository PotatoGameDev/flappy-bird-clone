using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlanetController : MonoBehaviour
{
    public float speed = 10f;
    public float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private Renderer rendr;
    private bool wasShown;

    // Audio sources
    public AudioSource screamsAudioSource;
    public AudioSource hitAudioSource;

    public AudioClip[] hitAudioClips;

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

    public void OnCollisionEnter2D(Collision2D collision)
    {
        SoundManager.Instance.PlayScreams(screamsAudioSource);
        SoundManager.Instance.PlayRandom(hitAudioSource, hitAudioClips);

    }
}
