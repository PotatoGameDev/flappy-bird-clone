using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed = 10f;

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.x += speed * Time.deltaTime;
        transform.position = pos;
    }
}
