using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Vector3 initialPosition;

    // The cam will slightly follow the player vertically,
    // but to a certain degree. 
    // This is the maximum the camera can go up or down:
    [SerializeField] private float maxVerticalFollowDistance = 1f;
    // This is the player max vertical speed that we will handle.
    // If the player is this fast up or down or more, the camera will go down
    // to maxVerticalFollowDistance up or down. 
    // If it's less, the camera will go proportionally less up or down.
    [SerializeField] private float maxVerticalTargetSpeed = 2f;
    // Min speed is to stop constant jumping:
    [SerializeField] private float minVerticalTargetSpeed = 0.5f;

    [SerializeField] private bool vertical = true;

    [SerializeField] private float verticalSmoothTime = 0.3f;
    private float verticalVelocity;

    void Awake()
    {
        initialPosition = transform.position;
    }

    void FixedUpdate()
    {
        PlanetController player = GameplayManager.Instance.Player;

        if (player.Dead)
            return;

        Vector3 pos = transform.position;
        pos.x += player.speed * Time.fixedDeltaTime;

        if (vertical)
        {
            float verticalSpeed = player.Velocity.y;

            float newY = 0f;

            if (player.Velocity.y > minVerticalTargetSpeed)
            {
                float deviationFraction = Mathf.Clamp01(
                        verticalSpeed / maxVerticalTargetSpeed
                        );

                newY = Mathf.Lerp(
                        0,
                        maxVerticalFollowDistance,
                        deviationFraction
                        );
            }

            float targetY = initialPosition.y + newY;

            pos.y = Mathf.SmoothDamp(
                transform.position.y,
                targetY,
                ref verticalVelocity,
                verticalSmoothTime
                );
        }

        transform.position = pos;
    }
}
