using UnityEngine;
using System.Collections.Generic;
using PotatoGameDev.Pool;

public class ObstaclesManager : MonoBehaviour
{
    public float minGap;
    public float verticalSpan;
    public GateController obstaclePrefab;
    public GateController obstacleDistantPrefab;

    public float vanishingDistance;

    private readonly List<GateController> obstacles = new();
    private readonly List<GateController> obstaclesDistant = new();

    private InstancePool<GateController> pool;
    private InstancePool<GateController> poolDistant;

    [SerializeField] private BossManager bossManager;

    void Awake()
    {
        pool = new InstancePool<GateController>(obstaclePrefab, transform);
        poolDistant = new InstancePool<GateController>(obstacleDistantPrefab, transform);
    }

    void FixedUpdate()
    {
        if (GameplayManager.Instance.Player.Dead)
        {
            return;
        }
        float playerPosX = GameplayManager.Instance.Player.transform.position.x;

        float lastObstaclePosX = obstacles.Count > 0 ? obstacles[^1].transform.position.x : 0f;


        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));

        float distToLastObstacle = playerPosX - lastObstaclePosX;

        float lastObstacleDistanceFromSpawn = spawnPos.x - lastObstaclePosX;

        if (obstacles.Count == 0 || lastObstacleDistanceFromSpawn > GetGap())
        {
            // Spawning next obstacle just outside of the camera view:
            spawnPos.z = 0f;
            spawnPos.y = Random.Range(-verticalSpan, verticalSpan);

            // Generating regular vanila gate
            GateController gateInst = pool.Get();
            gateInst.transform.position = spawnPos;
            gateInst.Init();

            obstacles.Add(gateInst);

            // And the final boss obstacles:
            if (bossManager.IsFinalBossActive())
            {
                Vector3 distantPos = spawnPos + bossManager.GetFinalBoss().PlayerOffset;
                distantPos.z = 0f;

                GateController gateDistantInst = poolDistant.Get();
                gateDistantInst.transform.position = distantPos;
                gateDistantInst.Init();

                obstaclesDistant.Add(gateDistantInst);
            }
        }
    }

    private float GetGap()
    {
        float currentSpeed = GameplayManager.Instance.Player.speed;
        float minSpeed = GameplayManager.Instance.Player.initialSpeed;

        float speedInc = currentSpeed - minSpeed;

        return Mathf.LerpUnclamped(minGap, 2 * minGap, speedInc / minSpeed);
    }

    void Update()
    {
        if (GameplayManager.Instance.Player.Dead)
        {
            return;
        }

        // Cleanup:
        for (int i = obstacles.Count - 1; i >= 0; i--)
        {
            GateController obj = obstacles[i];
            if (obj.transform.position.x + vanishingDistance
                    < GameplayManager.Instance.Player.transform.position.x)
            {
                obstacles.RemoveAt(i);
                obj.Release();
            }
        }
        for (int i = obstaclesDistant.Count - 1; i >= 0; i--)
        {
            GateController obj = obstaclesDistant[i];
            if (obj.transform.position.x + vanishingDistance
                    < GameplayManager.Instance.Player.transform.position.x)
            {
                obstaclesDistant.RemoveAt(i);
                obj.Release();
            }
        }
    }
}
