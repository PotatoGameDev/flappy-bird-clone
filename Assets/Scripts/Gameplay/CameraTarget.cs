using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    void FixedUpdate()
    {
        if (GameplayManager.Instance.Player.Dead)
            return;

        Vector3 pos = transform.position;
        pos.x += GameplayManager.Instance.Player.speed * Time.deltaTime;
        transform.position = pos;
    }
}
