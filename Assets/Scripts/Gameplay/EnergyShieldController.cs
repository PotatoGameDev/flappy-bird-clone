using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class EnergyShieldController : MonoBehaviour
{
    [SerializeField] private float shieldFlashingDurationSeconds = 1f;

    private readonly float flashSpeed = 0.2f;

    private float lastJumpedTime = 0f;

    [SerializeField] private float parryTimeMax = 0.2f; // How long after jumping the player will bounce the rockets fired by FinalBoss.
                                                        // Sort of invincibility frames.

    private SpriteRenderer shieldRenderer;
    private Coroutine shieldCoroutine;


    private Color originalColor;
    private Vector3 originalScale;

    [SerializeField] private Vector3 maxScale = Vector3.one * 1.3f;

    private AudioSource audioSource;
    [SerializeField] private AudioClip parrySound;

    private ContactFilter2D contactFilter;
    private readonly List<Collider2D> contacts = new(32);

    private enum ShieldState
    {
        OFF, SHIELD, PARRY
    }

    private ShieldState state;

    void Awake()
    {
        shieldRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        originalColor = shieldRenderer.color;
        originalScale = transform.localScale;

        state = ShieldState.OFF;
    }

    public void ActivateJumpShield()
    {
        lastJumpedTime = Time.time;

        if (shieldCoroutine != null)
        {
            StopAllCoroutines();
        }
        shieldCoroutine = StartCoroutine(ShieldParry());
    }

    public void ActivateProtectShield()
    {
        if (GameplayManager.Instance.ShieldAvailable())
        {
            if (state != ShieldState.PARRY)
            {
                state = ShieldState.SHIELD;

                if (shieldCoroutine != null)
                {
                    StopCoroutine(shieldCoroutine);
                }
                shieldCoroutine = StartCoroutine(ShieldFlashing());
            }
        }
    }

    public bool TryParry(IPlayerHitReceiver receiver)
    {
        if (!IsInShieldITime())
        {
            return false;
        }

        state = ShieldState.PARRY;

        audioSource.PlayOneShot(parrySound);

        receiver.PlayerHit(PlayerHitType.PARRY);

        return true;
    }

    private IEnumerator ShieldFlashing()
    {
        float elapsed = 0f;

        shieldRenderer.color = originalColor;
        transform.localScale = originalScale;

        while (elapsed < shieldFlashingDurationSeconds)
        {
            elapsed += Time.deltaTime;

            Color color = shieldRenderer.color;
            color.a = Mathf.PingPong(Time.time * flashSpeed, 1f);

            shieldRenderer.color = color;
            yield return null;
        }

        shieldRenderer.color = originalColor;
        shieldCoroutine = null;

        state = ShieldState.OFF;
    }

    private IEnumerator ShieldParry()
    {
        float elapsed = 0f;

        shieldRenderer.color = originalColor;
        transform.localScale = originalScale;

        while (elapsed < parryTimeMax)
        {
            elapsed += Time.deltaTime;
            float elapsedFraction = elapsed / parryTimeMax;

            Color color = Color.Lerp(originalColor, Color.red, elapsedFraction);
            shieldRenderer.color = color;

            transform.localScale = Vector3.Lerp(originalScale, maxScale, elapsedFraction);

            DamageNearbyEnemies(transform.localScale.magnitude);

            yield return null;
        }

        shieldRenderer.color = originalColor;
        transform.localScale = originalScale;

        shieldCoroutine = null;

        state = ShieldState.OFF;
    }


    private void DamageNearbyEnemies(float radius)
    {
        contacts.Clear();

        int count = Physics2D.OverlapCircle(transform.position, radius, contactFilter, contacts);

        for (int i = 0; i < count; i++)
        {
            if (contacts[i].TryGetComponent<IPlayerHitReceiver>(out var enemy))
            {
                enemy.PlayerHit(PlayerHitType.SHIELD);
            }
        }
    }

    // Right after pressing jump there is a short invincibility time
    // that reduces damage to the planet on contact with enemy
    // and increases the damage to the enemy.
    // Sort of "parry" mechanic.
    public bool IsInShieldITime()
    {
        return (Time.time - lastJumpedTime) < parryTimeMax;
    }
}
