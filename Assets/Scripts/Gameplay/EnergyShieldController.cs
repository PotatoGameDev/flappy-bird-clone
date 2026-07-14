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

    private Coroutine shieldParryCoroutine;

    private Color originalColor;

    private AudioSource audioSource;
    [SerializeField] private AudioClip parrySound;

    private CircleCollider2D playerCollider;
    private Rigidbody2D playerRb;

    private ContactFilter2D contactFilter;
    private readonly List<Collider2D> contacts = new(32);

    private DamageCalculator damageCalculator;

    private enum ShieldState
    {
        OFF, SHIELD, PARRY
    }

    private ShieldState state;

    [SerializeField]
    private float shieldAlphaMax = 0.5f;

    public float GetBaseRadius()
    {
        return transform.localScale.magnitude;
    }

    void Awake()
    {
        shieldRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        playerCollider = GetComponentInParent<CircleCollider2D>();
        playerRb = GetComponentInParent<Rigidbody2D>();

        damageCalculator = GetComponentInParent<DamageCalculator>();
    }

    void Start()
    {
        originalColor = shieldRenderer.color;

        state = ShieldState.OFF;
    }

    void Update()
    {
        float shieldLevelRatio = GameplayManager.Instance.ShieldAvailableRatio();

        Color rendererColor = shieldRenderer.color;
        rendererColor.a = shieldLevelRatio * shieldAlphaMax;

        shieldRenderer.color = rendererColor;
    }

    internal void RegisterJump()
    {
        lastJumpedTime = Time.time;

        // There could be 2 aproaches:
        // - hit the enemies inside the shield interval
        // - hit the enemies once on every jump.
        // I chose the latter, we'll se how it plays.

        if (GameplayManager.Instance.ShieldAvailable())
        {
            DamageNearbyEnemies();
        }
    }

    public void ActivateJumpShieldVisual(bool parry)
    {
        if (shieldParryCoroutine != null)
        {
            StopAllCoroutines();
            shieldParryCoroutine = null;
            shieldCoroutine = null;
        }
        shieldParryCoroutine = StartCoroutine(ShieldParry(parry));
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

    public void TryParry()
    {
        /*
        if (!IsInShieldITime())
        {
            return;
        }
        */

        state = ShieldState.PARRY;

        audioSource.PlayOneShot(parrySound);
    }

    private IEnumerator ShieldFlashing()
    {
        float elapsed = 0f;

        shieldRenderer.color = originalColor;

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

    private IEnumerator ShieldParry(bool parry)
    {
        float elapsed = 0f;

        shieldRenderer.color = originalColor;

        Color finalColor = parry ? Color.red : Color.blue;

        while (elapsed < parryTimeMax)
        {
            elapsed += Time.deltaTime;
            float elapsedFraction = elapsed / parryTimeMax;

            Color color = Color.Lerp(originalColor, finalColor, elapsedFraction);
            shieldRenderer.color = color;

            // This changed to single check on jump:
            //DamageNearbyEnemies(transform.localScale.magnitude);

            yield return null;
        }

        shieldRenderer.color = originalColor;

        shieldParryCoroutine = null;

        state = ShieldState.OFF;
    }


    private void DamageNearbyEnemies()
    {
        contacts.Clear();

        int count = Physics2D.OverlapCircle(
                transform.position,
                GetBaseRadius(),
                contactFilter,
                contacts);

        long shieldUsed = 0L;
        bool parried = false;

        for (int i = 0; i < count; i++)
        {
            if (contacts[i].TryGetComponent<IPlayerHitReceiver>(out var enemy))
            {
                if (!enemy.CanBeDamaged())
                {
                    continue;
                }

                Rigidbody2D enemyRb = contacts[i].attachedRigidbody;

                Vector2 relativeVelocity = enemyRb.linearVelocity
                    - playerRb.linearVelocity;

                Damage damage = damageCalculator.CalculateDamage(
                        contacts[i].Distance(playerCollider).distance,
                        enemy.GetLifeUnit(),
                        relativeVelocity,
                        true
                        );

                enemy.PlayerHit(damage);

                shieldUsed += damage.enemy;
                if (damage.parried)
                {
                    parried = true;
                }
            }
        }

        if (shieldUsed > 0L)
        {
            GameplayManager.Instance.UseUpShield(shieldUsed);
            ActivateJumpShieldVisual(parried);
            if (parried)
            {
                audioSource.PlayOneShot(parrySound);
            }
        }
    }

    // Right after pressing jump there is a short invincibility time
    // that reduces damage to the planet on contact with enemy
    // and increases the damage to the enemy.
    // Sort of "parry" mechanic.
    // EDIT: No more time based, purely distance based.
    // TODO: maybe this could be used for coyoting the parry.
    public bool IsInShieldITime()
    {
        return false;
        //return (Time.time - lastJumpedTime) < parryTimeMax;
    }
}
