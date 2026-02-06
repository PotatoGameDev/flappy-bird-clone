using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    void FixedUpdate()
    {
        Vector3 pos = new Vector3(target.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, pos, 0.1f);
    }
}
