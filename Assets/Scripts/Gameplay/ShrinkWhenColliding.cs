using UnityEngine;

// Makes an object bigger or smaller visually as it passes an obstacle around
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShrinkWhenColliding : MonoBehaviour
{
    private SpriteRenderer rendr;
    private CircleCollider2D circleCollider;
    private Vector2 originalScale;

    [SerializeField] private Vector2 addedScale;

    private Vector2 targetScale;

    private int originalSortingOrder;

    private bool collidingWithGate;

    void Awake()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        rendr = GetComponent<SpriteRenderer>();

        originalScale = transform.localScale;
        targetScale = originalScale;
        originalSortingOrder = rendr.sortingOrder;
    }

    void Update()
    {
        if (collidingWithGate)
        {
            float direction = GetDirection(circleCollider);
            float dist = Mathf.Abs(transform.position.x - circleCollider.transform.position.x);
            targetScale = Vector2.Lerp(originalScale + (direction * addedScale), originalScale, dist / circleCollider.radius);
        }

        if ((Vector2)transform.localScale != targetScale)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, targetScale, 10 * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("GatePipe"))
        {
            // The ufos will pass the obstacles from the front on the bottom of the screen,
            // and from the back on the top of the screen, like they are circling
            float direction = GetDirection(collider);
            rendr.sortingOrder = originalSortingOrder + (int)(direction * 5);

            collidingWithGate = true;
        }
    }

    private float GetDirection(Collider2D collider)
    {
        return -Mathf.Sign(collider.transform.position.y);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("GatePipe"))
        {
            targetScale = originalScale;

            collidingWithGate = false;
        }
    }
}
