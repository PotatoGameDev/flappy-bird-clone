using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [SerializeField] private Vector3 initialPosition;

    [SerializeField] private float maxVerticalFollowDistance = 1f;
    [SerializeField] private float verticalSmoothTime = 0.3f;
    [SerializeField] private float verticalDeadzone = 2f;

    private float verticalVelocity;


    public float DisplacementY
    {
        get
        {
            return transform.position.y - initialPosition.y;
        }
    }

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

        float playerY = player.transform.position.y;

        if (Mathf.Abs(playerY) > verticalDeadzone)
        {
            float sign = Mathf.Sign(playerY);
            float distBeyondDeadZone = Mathf.Abs(playerY) - verticalDeadzone;

            float deviationFraction = Mathf.Clamp(
                    distBeyondDeadZone / maxVerticalFollowDistance,
                    0,
                    1
                    );

            float newY = maxVerticalFollowDistance * deviationFraction * sign;

            float targetY = initialPosition.y + newY;

            pos.y = Mathf.SmoothDamp(
                transform.position.y,
                targetY,
                ref verticalVelocity,
                verticalSmoothTime
                );
        }
        else
        {
            pos.y = Mathf.SmoothDamp(
                transform.position.y,
                initialPosition.y,
                ref verticalVelocity,
                verticalSmoothTime
            );
        }

        transform.position = pos;
    }
}
