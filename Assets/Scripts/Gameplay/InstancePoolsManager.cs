using UnityEngine;
using PotatoGameDev.Pool;

public class InstancePoolsManager : MonoBehaviour
{
    public static InstancePoolsManager Instance { get; private set; }

    [SerializeField] private EnergyBallController energyBallControllerPrefab;
    public InstancePool<EnergyBallController> EnergyBallControllerPool { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Non singleton stuff
        EnergyBallControllerPool = new(energyBallControllerPrefab, 20);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
