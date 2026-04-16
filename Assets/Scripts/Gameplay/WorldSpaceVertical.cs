using UnityEngine;

public class WorldSpaceVertical : MonoBehaviour
{
    private float initialWorldY;

    void Start()
    {
        initialWorldY = transform.position.y;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = initialWorldY;
        transform.position = pos;
    }
}
