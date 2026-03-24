using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;

    void FixedUpdate()
    {
        if (GameplayManager.Instance.Player.Dead)
        {
            return;
        }

        Vector3 pos = transform.position;
        pos.x += GameplayManager.Instance.Player.speed * Time.fixedDeltaTime;
        transform.position = pos;
    }
}


