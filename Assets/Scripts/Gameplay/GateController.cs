using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PotatoGameDev.Pool;

public class GateController : PooledInstance
{
    [SerializeField] private Transform topEnergyBallSpawner;
    [SerializeField] private Transform bottomEnergyBallSpawner;

    [SerializeField] private EnergyBallController energyBallPrefab;


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            StartCoroutine(GenerateEnergyBalls());
        }
    }

    private IEnumerator GenerateEnergyBalls()
    {
        int energyPerGate = GameplayManager.Instance.EnergyPerGate();

        float timePerEnergy = 0.5f /* sec */ / energyPerGate;

        Stack<EnergyBallController> energyPortions = EnergyBallManager.Instance.GetForTotal(energyPerGate);
        foreach (EnergyBallController energy in energyPortions)
        {
            energy.Type = EnergyType.PipeEnergy;
        }

        int i = 0;
        while (energyPortions.Count > 0)
        {
            if (i % 2 == 0)
            {
                EnergyBallController ball = energyPortions.Pop();
                ball.Init();

                ball.transform.position = topEnergyBallSpawner.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0f);
                ball.Type = EnergyType.PipeEnergy;
                yield return new WaitForSeconds(timePerEnergy / 2f);
            }
            else
            {
                EnergyBallController ball = energyPortions.Pop();
                ball.Init();
                ball.transform.position = bottomEnergyBallSpawner.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0f);
                ball.Type = EnergyType.PipeEnergy;
                yield return new WaitForSeconds(timePerEnergy / 2f);
            }
            i++;
        }
    }

    private void CreateBall(Transform source)
    {
    }
}
