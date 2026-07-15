using UnityEngine;
using PotatoGameDev.Pool;

public class InstancePoolsManager : MonoBehaviour
{
    public static InstancePoolsManager Instance
    { get; private set; }

    [SerializeField] internal EnergyBallController energyBallControllerPrefab;
    public InstancePool<EnergyBallController> EnergyBallControllerPool
    { get; private set; }

    [SerializeField] private BulletController bulletControllerPrefab;
    public InstancePool<BulletController> BulletControllerPool
    { get; private set; }

    [SerializeField] private ExplosionController explosionControllerPrefab;
    public InstancePool<ExplosionController> ExplosionControllerPool
    { get; private set; }

    [SerializeField] private RocketController rocketControllerPrefab;
    [SerializeField] private RocketController tinyRocketControllerPrefab;

    public InstancePool<RocketController> RocketControllerPool
    { get; private set; }
    public InstancePool<RocketController> TinyRocketControllerPool
    { get; private set; }

    public void ReleaseAll()
    {
        EnergyBallControllerPool.ReleaseAll();
        BulletControllerPool.ReleaseAll();
        ExplosionControllerPool.ReleaseAll();
        RocketControllerPool.ReleaseAll();
        TinyRocketControllerPool.ReleaseAll();
    }

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

        // Create the new pool, setting this transform as a parent for 
        // the pooled instances,
        // otherwise they will be erased when the scene unloads
        EnergyBallControllerPool = new(energyBallControllerPrefab, transform);

        BulletControllerPool = new(bulletControllerPrefab, transform);

        ExplosionControllerPool = new(explosionControllerPrefab, transform);

        RocketControllerPool = new(rocketControllerPrefab, transform);

        TinyRocketControllerPool = new(tinyRocketControllerPrefab, transform);
    }
}
