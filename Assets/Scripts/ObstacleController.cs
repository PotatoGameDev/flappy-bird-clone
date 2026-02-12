using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private bool passed;

    void Update()
    {
        Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
        if (transform.position.x < pos.x)
        {

        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!passed && collider.CompareTag("Player"))
        {
            GameplayManager.Instance.gateCount++;
            passed = true;
        }
    }
}
