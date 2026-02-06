using UnityEngine;

public class ObstaclesManager : MonoBehaviour
{
    public float minGap;
    public float horizontalSpan;
    public GameObject obstaclePrefab;
    private GameObject lastObstacle;


    void FixedUpdate()
    {
        float playerPosX = GameManager.Instance.Player.transform.position.x;
        float lastObstaclePosX = lastObstacle == null ? 0f : lastObstacle.transform.position.x;
        float distToLastObstacle = playerPosX - lastObstaclePosX;

        if (distToLastObstacle > 0 && distToLastObstacle > minGap)
        {
            // Spawning next obstacle just outside of the camera view:
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
            pos.z = 0f;
            pos.y = Random.Range(-horizontalSpan, horizontalSpan);
            lastObstacle = Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);
        }
    }
}
