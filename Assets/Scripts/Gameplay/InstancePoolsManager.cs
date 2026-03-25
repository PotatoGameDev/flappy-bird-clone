using UnityEngine;
using PotatoGameDev.Pool;

public class InstancePoolsManager : MonoBehaviour
{
    public static InstancePoolsManager Instance { get; private set; }

    [SerializeField] private EnergyBallController energyBallControllerPrefab;
    public InstancePool<EnergyBallController> EnergyBallControllerPool { get; private set; }

    [SerializeField] private BulletController bulletControllerPrefab;
    public InstancePool<BulletController> BulletControllerPool { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Non singleton stuff

        // Create the new pool, setting this transform as a parent for the pooled instances,
        // otherwise they will be erased when the scene unloads
        EnergyBallControllerPool = new(energyBallControllerPrefab, transform);

        BulletControllerPool = new(bulletControllerPrefab, transform);
    }
}
