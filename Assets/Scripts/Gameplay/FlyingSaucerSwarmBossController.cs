using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SwarmController))]
public class FlyingSaucerSwarmBossController : MonoBehaviour
{
    private SwarmController swarmController;
    private float totalBossHealth;

    void Awake()
    {
        swarmController = GetComponent<SwarmController>();

        swarmController.SwarmBoidDied += OnSwarmChange;
    }

    void Start()
    {
        totalBossHealth = 0f;
        foreach (SwarmFollow boid in swarmController.Boids)
        {
            boid.TryGetComponent(out FlyingSaucerController flyingSaucer);
            if (flyingSaucer != null)
            {
                totalBossHealth += flyingSaucer.CurrentLife;
                flyingSaucer.DamageTaken += OnSwarmChange;
                continue;
            }

            boid.TryGetComponent(out MotherShipperController motherShipper);
            if (motherShipper != null)
            {
                totalBossHealth += motherShipper.CurrentLife;
                motherShipper.DamageTaken += OnSwarmChange;
                continue;
            }
        }

        GameplayManager.Instance.SetBossHealth(totalBossHealth, totalBossHealth);
    }

    private void OnSwarmChange()
    {
        float currentTotal = GetTotalHealth(swarmController.Boids);

        GameplayManager.Instance.SetBossHealth(currentTotal, totalBossHealth);
    }

    private float GetTotalHealth(List<SwarmFollow> boids)
    {
        float total = 0f;
        foreach (SwarmFollow boid in boids)
        {
            if (boid != null && boid.enabled && boid.gameObject.activeInHierarchy)
            {
                boid.TryGetComponent(out FlyingSaucerController flyingSaucer);
                if (flyingSaucer != null)
                {
                    total += flyingSaucer.CurrentLife;
                }
                else
                {
                    boid.TryGetComponent(out MotherShipperController motherShipper);
                    if (motherShipper != null)
                    {
                        total += motherShipper.CurrentLife;
                    }
                }
            }
        }
        return total;
    }
}
