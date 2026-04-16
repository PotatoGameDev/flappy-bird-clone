using UnityEngine;

public class ParallaxVertical : MonoBehaviour
{
    [SerializeField] private float verticalFactor;
    [SerializeField] private CameraTarget cameraTarget;
    private Vector2 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector2 pos = transform.position;
        pos.y = initialPosition.y - verticalFactor * cameraTarget.DisplacementY;

        transform.position = pos;
    }
}
