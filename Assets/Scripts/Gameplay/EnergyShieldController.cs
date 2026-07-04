using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class EnergyShieldController : MonoBehaviour
{
    [SerializeField] private float shieldFlashingDurationSeconds = 1f;
    [SerializeField] private float parryExpandTime = 0.5f;

    private readonly float flashSpeed = 0.2f;

    private float lastJumpedTime = 0f;
    [SerializeField]
    private float parryTimeMax = 0.2f; // How long after jumping the player will bounce the rockets fired by FinalBoss.
                                       // Sort of invincibility frames.

    private SpriteRenderer shieldRenderer;
    private Coroutine shieldCoroutine;


    private Color originalColor;
    private Vector3 originalScale;

    [SerializeField] private Vector3 maxScale = Vector3.one * 1.3f;

    private enum ShieldState
    {
        OFF, SHIELD, PARRY
    }

    private ShieldState state;

    void Awake()
    {
        shieldRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        originalColor = shieldRenderer.color;
        originalScale = transform.localScale;

        state = ShieldState.OFF;
    }

    public void RegisterJump()
    {
        lastJumpedTime = Time.time;
    }

    public void StartFlashing()
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

    public bool TryParry()
    {
        if (!IsInShieldITime())
        {
            return false;
        }

        // TODO: Add satisfying 'parry' sound
        state = ShieldState.PARRY;

        shieldCoroutine = StartCoroutine(ShieldParry());

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

        while (elapsed < parryExpandTime)
        {
            elapsed += Time.deltaTime;

            Color color = shieldRenderer.color;
            Color red = Color.red;

            color = Color.Lerp(color, red, parryExpandTime * Time.deltaTime);
            shieldRenderer.color = color;

            Vector3 localScale = transform.localScale;
            transform.localScale = Vector3.Lerp(localScale, maxScale, parryExpandTime * Time.deltaTime);

            yield return null;
        }

        shieldRenderer.color = originalColor;
        shieldCoroutine = null;

        state = ShieldState.OFF;
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
