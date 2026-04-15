using UnityEngine;

public class ParallaxVertical : MonoBehaviour
{
    [SerializeField] private float verticalFactor;
    [SerializeField] private Transform cameraTarget;

    void FixedUpdate()
    {
        Vector2 pos = transform.position;
        pos.y = verticalFactor * cameraTarget.transform.position.y;

        transform.position = pos;
    }
}
