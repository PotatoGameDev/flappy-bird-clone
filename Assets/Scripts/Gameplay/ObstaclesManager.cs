using UnityEngine;
using System.Collections.Generic;
using PotatoGameDev.Pool;

public class ObstaclesManager : MonoBehaviour
{
    public float minGap;
    public float verticalSpan;
    public GateController obstaclePrefab;

    public float vanishingDistance;

    private readonly List<GateController> obstacles = new();

    private InstancePool<GateController> pool;

    void Awake()
    {
        pool = new InstancePool<GateController>(obstaclePrefab, transform);
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

            GateController gateInst = pool.Get();
            gateInst.transform.position = pos;
            gateInst.Init();

            obstacles.Add(gateInst);
        }
    }

    void Update()
    {
        if (GameplayManager.Instance.Player.Dead)
        {
            return;
        }
        // Cleanup:
        for (int i = 0; i < obstacles.Count; i++)
        {
            GateController obj = obstacles[i];
            if (obj.transform.position.x + vanishingDistance
                    < GameplayManager.Instance.Player.transform.position.x)
            {
                obstacles.RemoveAt(i);
                obj.Release();
            }
        }
    }
}
