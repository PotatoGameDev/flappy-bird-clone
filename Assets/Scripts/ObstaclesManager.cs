using UnityEngine;
using System.Collections.Generic;

public class ObstaclesManager : MonoBehaviour
{
    public float minGap;
    public float horizontalSpan;
    public GameObject obstaclePrefab;

    public float vanishingDistance;

    private List<GameObject> obstacles = new();

    void FixedUpdate()
    {
        float playerPosX = GameManager.Instance.Player.transform.position.x;

        float lastObstaclePosX = obstacles.Count > 0 ? obstacles[^1].transform.position.x : 0f;

        float distToLastObstacle = playerPosX - lastObstaclePosX;

        if (distToLastObstacle > 0 && distToLastObstacle > minGap)
        {
            // Spawning next obstacle just outside of the camera view:
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
            pos.z = 0f;
            pos.y = Random.Range(-horizontalSpan, horizontalSpan);
            GameObject obj = Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);

            obstacles.Add(obj);
        }
    }

    void Update()
    {
        // Cleanup:
        for (int i = 0; i < obstacles.Count; i++)
        {
            GameObject obj = obstacles[i];
            if (obj.transform.position.x + vanishingDistance
                    < GameManager.Instance.Player.transform.position.x)
            {
                obstacles.RemoveAt(i);
                Destroy(obj);
            }
        }
    }
}
