using UnityEngine;
using System.Collections;
using PotatoGameDev.Pool;

public class GateController : PooledInstance
{
    [SerializeField] private Transform topEnergyBallSpaner;
    [SerializeField] private Transform bottomEnergyBallSpaner;

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

        for (int i = 0; i < energyPerGate; i++)
        {
            if (i % 2 == 0)
            {
                CreateBall(topEnergyBallSpaner);
                yield return new WaitForSeconds(timePerEnergy / 2f);
            }
            else
            {
                CreateBall(bottomEnergyBallSpaner);
                yield return new WaitForSeconds(timePerEnergy / 2f);
            }
        }
    }

    private void CreateBall(Transform source)
    {
        EnergyBallController ball = InstancePoolsManager.Instance.EnergyBallControllerPool.Get();
        ball.Init();

        ball.transform.position = source.position + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0f);
        ball.FollowPlayer = true;
    }
}
