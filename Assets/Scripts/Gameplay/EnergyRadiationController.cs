using UnityEngine;
using System.Collections;

public class EnergyRadiationController : MonoBehaviour
{

    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform destinationPosition;
    [SerializeField] private Vector2 destinationSpread;
    [SerializeField] private float frequencySeconds = 1f;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private UpgradeId upgradeId;

    void Start()
    {
        int energyCount = UpgradesManager.Instance.GetUpgrade(upgradeId).Level;
        if (energyCount > 0)
        {
            StartCoroutine(GenerateEnergyParticles(energyCount));
        }
    }

    public IEnumerator GenerateEnergyParticles(int energyCount)
    {
        while (true)
        {
            float timePerEnergy = 1f /* sec */ / energyCount;

            for (int i = 0; i < energyCount; i++)
            {
                EnergyBallController ball = InstancePoolsManager.Instance.EnergyBallControllerPool.Get();
                ball.Init();
                ball.transform.position = spawnPosition.position
                    + new Vector3(Random.Range(-1, 2), Random.Range(-1, 2), 0f);

                Vector3 destination = destinationPosition.position + new Vector3(Random.Range(0, destinationSpread.x), Random.Range(0, destinationSpread.y));

                while (!ball.CanPlace(destination, layerMask))
                {
                    destination += Vector3.right * 0.01f;
                }


                ball.Target = destination;


                ball.FollowTarget = true;

                yield return new WaitForSeconds(timePerEnergy / 2f);
            }
            yield return new WaitForSeconds(frequencySeconds + Random.Range(0, 2));
        }
    }
}
