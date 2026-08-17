using UnityEngine;
using System.Collections;
using PotatoGameDev.Pool;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BulletController : PooledInstance, IPlayerHitReceiver
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] internal float speed = 5;
    [SerializeField] private float lifetime = 5;

    private Vector2 direction;

    private SpriteRenderer rendr;

    private WaitForSeconds WAIT_LIFETIME;
    private Coroutine timeoutCoroutine;

    public bool alive;

    void Awake()
    {
        rendr = GetComponent<SpriteRenderer>();

        WAIT_LIFETIME = new(lifetime);
    }

    public void FromTo(Vector2 from, Vector2 to)
    {
        direction = (to - from).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public new void Init()
    {
        rendr.sprite = sprites[Random.Range(0, sprites.Length)];
        rendr.enabled = true;
        Debug.Log("enabling renderer");
        transform.localScale = Vector2.one * 0.1f;
        timeoutCoroutine = StartCoroutine(SelfDestruct());
        alive = true;
    }

    void FixedUpdate()
    {
        Vector2 translation = direction * (speed * Time.fixedDeltaTime);
        transform.Translate(translation, Space.World);

        if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one, Time.fixedDeltaTime * 10);
        }
    }

    IEnumerator SelfDestruct()
    {
        yield return WAIT_LIFETIME;
        Release();
    }

    new void Release()
    {
        rendr.enabled = false;
        Debug.Log("Disabling renderer");
        alive = false;
        StopCoroutine(timeoutCoroutine);
        base.Release();
        // TODO maybe stop all coroutines in base?
    }

    public void PlayerHit(Damage damage)
    {
        Release();
    }

    public bool IsHittable() => true;

    public long GetLifeUnit() => (long)speed;
}
