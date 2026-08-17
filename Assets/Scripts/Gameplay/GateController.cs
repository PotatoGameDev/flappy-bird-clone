using UnityEngine;
using System.Collections;
using PotatoGameDev.Pool;

public class GateController : PooledInstance
{
    [SerializeField] private Transform topEnergyBallSpawner;
    [SerializeField] private Transform bottomEnergyBallSpawner;

    [SerializeField] private Transform topTarget;
    [SerializeField] private Transform bottomTarget;

    [SerializeField] private Vector2 targetSpread = Vector2.one;

    [SerializeField] private bool generateEnergy;

    [SerializeField] private float chancesMultiplier = 1.0f;


    public new void Init()
    {
        if (generateEnergy)
        {
            StartCoroutine(GenerateEnergyBalls());
        }

        base.Init();
    }

    public new void Release()
    {
        StopAllCoroutines();
        base.Release();
    }


    private IEnumerator GenerateEnergyBalls()
    {
        int energyPerGate = UpgradesManager.Instance.GetORingEnergyPerLevel();

        float timePerEnergy = 0.5f /* sec */ / energyPerGate;

        for (int i = 0; i < energyPerGate; i++)
        {
            EnergyBallController ball = EnergyBallManager.Instance
                .GetRandomForSpeed(
                    GameplayManager.Instance.Player.speed * chancesMultiplier
                    );
            if (i % 2 == 0)
            {
                ball.Init(bottomEnergyBallSpawner.position
                    + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0f));
                ball.SetTargetVector(bottomTarget.position + (Vector3)(Random.insideUnitCircle * targetSpread));
            }
            else
            {
                ball.Init(topEnergyBallSpawner.position
                    + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0f));
                ball.SetTargetVector(topTarget.position + (Vector3)(Random.insideUnitCircle * targetSpread));
            }

            ball.Type = EnergyType.CollectEnergy;

            yield return new WaitForSeconds(timePerEnergy / 2f); // Divide by 2 because it's split
        }
    }
}
