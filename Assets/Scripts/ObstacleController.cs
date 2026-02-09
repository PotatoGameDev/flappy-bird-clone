using UnityEngine;

public class ObstacleController : MonoBehaviour
{

    void Awake()
    {

    }

    void Update()
    {
        Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 1f));
        if (transform.position.x < pos.x)
        {

        }
    }
}
