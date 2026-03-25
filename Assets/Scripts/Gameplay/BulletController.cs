using UnityEngine;
using System.Collections;
using PotatoGameDev.Pool;

[RequireComponent(typeof(SpriteRenderer))]
public class BulletController : PooledInstance
{
    [SerializeField] private Sprite[] sprites;
    [SerializeField] internal float speed = 5;
    [SerializeField] private float lifetime = 5;

    private Vector2 direction;

    private SpriteRenderer rendr;

    private WaitForSeconds WAIT_LIFETIME;
    private Coroutine timeoutCoroutine;

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
        timeoutCoroutine = StartCoroutine(SelfDestruct());
    }

    void FixedUpdate()
    {
        Vector2 translation = direction * (speed * Time.fixedDeltaTime);
        transform.Translate(translation, Space.World);
    }

    IEnumerator SelfDestruct()
    {
        yield return WAIT_LIFETIME;
        Release();
    }

    new void Release()
    {
        StopCoroutine(timeoutCoroutine);
        base.Release();
        // TODO maybe stop all coroutines in base?
    }

}
