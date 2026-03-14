using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public bool Passed { get; set; }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!Passed && collider.CompareTag("Player"))
        {
            GameplayManager.Instance.CollectEnergy();
            Passed = true;
        }
    }
}
