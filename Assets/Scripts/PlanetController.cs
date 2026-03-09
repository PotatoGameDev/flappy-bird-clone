using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlanetController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float jumpForce = 10f;
    private float currentJumpForce = 0f;

    private Rigidbody2D rb;
    private SpriteRenderer rendr;

    [SerializeField]
    private float invincibilityDurationSeconds = 2f;
    [SerializeField]
    private float flashSpeed = 1f;
    private bool invincible = false;

    // Audio sources
    [SerializeField]
    private float screamsVolume = 0.5f;
    [SerializeField]
    private AudioSource screamsAudioSource;
    [SerializeField]
    private float hitVolume = 1f;
    [SerializeField]
    private AudioSource hitAudioSource;
    [SerializeField]
    private float quakeVolume = 0.5f;
    [SerializeField]
    private AudioSource quakeAudioSource;

    [SerializeField]
    private AudioClip[] hitAudioClips;
    [SerializeField]
    private AudioClip[] quakeAudioClips;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendr = GetComponentInChildren<SpriteRenderer>();

        GameManager.Instance.Player = this;
    }

    void Update()
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

    // Controls:

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            currentJumpForce += jumpForce;
        }
    }

    public void OnBack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            GameplayManager.Instance.Death();
        }
    }

    // Collisions:

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
