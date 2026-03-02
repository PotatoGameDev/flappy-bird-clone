using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private bool passed;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!passed && collider.CompareTag("Player"))
        {
            GameplayManager.Instance.CollectEnergy();
            passed = true;
        }
    }
}
