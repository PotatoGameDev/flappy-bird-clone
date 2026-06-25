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
    public static Queue<SpawnLog> spawnQueue = new();

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

        float distToLastObstacle = playerPosX - lastObstaclePosX;

        if (distToLastObstacle > 0 && distToLastObstacle > minGap)
        {
            // Spawning next obstacle just outside of the camera view:
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
            pos.z = 0f;
            pos.y = Random.Range(-verticalSpan, verticalSpan);

            // Generating regular vanila gate
            GateController gateInst = pool.Get();
            gateInst.transform.position = pos;
            gateInst.Init();

            obstacles.Add(gateInst);
            RegisterSpawn(pos);

        }

        // Generating the 'evil' gate, for the boss, if needed
        if (bossManager.IsFinalBossActive())
        {
            if (spawnQueue.Count > 0)
            {
                var entry = spawnQueue.Peek();

                if (Time.time - entry.timestamp >= bossManager.GetFinalBoss().GetDelaySeconds())
                {
                    spawnQueue.Dequeue();

                    // We want previous spawn point y, but new x
                    // So we have normally spawned gate, some defined time after the last one.
                    Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
                    position.z = 0f;
                    position.y = entry.position.y;

                    GateController gateDistantInst = poolDistant.Get();
                    gateDistantInst.transform.position = position;
                    gateDistantInst.Init();

                    obstaclesDistant.Add(gateDistantInst);
                }
            }

        }
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

    public void RegisterSpawn(Vector3 position)
    {
        spawnQueue.Enqueue(new SpawnLog
        {
            position = position,
            timestamp = Time.time,
        });
    }
}

public struct SpawnLog
{
    public float timestamp;
    public Vector3 position;
}
