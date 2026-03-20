using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private BoxCollider2D topBoundary;
    [SerializeField] private BoxCollider2D leftBoundary;
    [SerializeField] private BoxCollider2D bottomBoundary;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        if (GameplayManager.Instance.Player.Dead)
        {
            return;
        }

        Vector3 pos = transform.position;
        pos.x += GameplayManager.Instance.Player.speed * Time.deltaTime;
        transform.position = pos;
    }

    void Update()
    {
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        float thickness = 1f;

        Vector3 camPos = cam.transform.position;

        topBoundary.size = new Vector2(width, thickness);
        topBoundary.offset = new Vector2(0f, 1f);
        topBoundary.transform.position = new Vector3(camPos.x, camPos.y + height / 2 + thickness / 2, 0f);

        bottomBoundary.size = new Vector2(width, thickness);
        bottomBoundary.offset = new Vector2(0f, -1f);
        bottomBoundary.transform.position = new Vector3(camPos.x, camPos.y - height / 2 - thickness / 2, 0f);

        leftBoundary.size = new Vector2(thickness, height);
        leftBoundary.transform.position = new Vector3(camPos.x - width / 2 - thickness / 2, camPos.y, 0f);
    }
}
