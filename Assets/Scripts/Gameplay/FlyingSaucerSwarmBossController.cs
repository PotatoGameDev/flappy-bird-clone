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
            FlyingSaucerController flyingSaucer = boid.GetComponent<FlyingSaucerController>();
            totalBossHealth += flyingSaucer.CurrentLife;
            flyingSaucer.DamageTaken += OnSwarmChange;
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
            FlyingSaucerController flyingSaucer = boid.GetComponent<FlyingSaucerController>();
            total += flyingSaucer.CurrentLife;
        }
        return total;
    }
}
