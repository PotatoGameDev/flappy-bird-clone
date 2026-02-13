using System.Collections;
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
    private SpriteRenderer rendr;
    private bool wasShown;

    public float invincibilityDurationSeconds = 2f;
    public float flashSpeed = 1f;
    private bool invincible = false;

    // Audio sources
    public float screamsVolume = 0.5f;
    public AudioSource screamsAudioSource;
    public float hitVolume = 1f;
    public AudioSource hitAudioSource;
    public float quakeVolume = 0.5f;
    public AudioSource quakeAudioSource;

    public AudioClip[] hitAudioClips;
    public AudioClip[] quakeAudioClips;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();

        GameManager.Instance.Player = this;
    }

    void Update()
    {
        if (rendr.isVisible)
        {
            // TODO: Not a good way to detect GameOver.
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
        if (invincible) return;

        StartCoroutine(IFrames());

        SoundManager.Instance.PlayScreams(screamsAudioSource, screamsVolume);
        SoundManager.Instance.PlayRandom(quakeAudioSource, quakeAudioClips, quakeVolume);
        SoundManager.Instance.PlayRandom(hitAudioSource, hitAudioClips, hitVolume);

        GameplayManager.Instance.TakeHit(collision.relativeVelocity.magnitude);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BoundaryBack"))
        {
            GameplayManager.Instance.Death();
        }
    }

    public int GetRpm()
    {
        return (int)Mathf.Abs(rb.angularVelocity / 6);
    }

    private IEnumerator IFrames()
    {
        invincible = true;
        float elapsed = 0f;
        Color originalColor = rendr.color;

        while (elapsed < invincibilityDurationSeconds)
        {
            elapsed += Time.deltaTime;

            Color color = rendr.color;
            color.a = Mathf.PingPong(Time.time * flashSpeed, 1f);

            rendr.color = color;
            yield return null;
        }

        rendr.color = originalColor;
        invincible = false;
    }
}
