using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Events;

public class PathFollow : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new();

    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private float waypointGizmoRadius = 0.2f;
    [SerializeField] private LoopType loopType = LoopType.Incremental;
    [SerializeField] private float speed = 0.4f;
    [SerializeField] private Ease easeType = Ease.InOutSine;
    [SerializeField] private int curveResolution = 50;
    [SerializeField] private bool loopPath = true;

    [SerializeField] private UnityEvent<int> onWaypointReached;

    private Tween moveTween;

    void Start()
    {
        BuildSequence();
    }

    void BuildSequence()
    {
        Debug.Assert(waypoints.Count >= 2, "Waypoints not added");

        transform.localPosition = waypoints[0].localPosition;

        moveTween = transform.DOLocalPath(
                GetPositions(),
                speed,
                PathType.CatmullRom
        )
        .SetSpeedBased()
        .SetEase(easeType)
        .SetOptions(loopPath)
        .SetLoops(-1, loopType)
        .OnWaypointChange(OnWaypointReached);
    }

    void OnWaypointReached(int waypointIndex)
    {
        onWaypointReached?.Invoke(waypointIndex);
    }

    void OnDestroy()
    {
        moveTween?.Kill();
    }

    public Vector3[] GetPositions(bool loop = false)
    {
        Vector3[] positions = new Vector3[waypoints.Count];

        for (int i = 0; i < waypoints.Count; i++)
        {
            positions[i] = waypoints[i].localPosition;
        }

        if (loop)
        {
            positions[^1] = waypoints[0].localPosition;
        }

        return positions;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float tt = t * t;
        float ttt = tt * t;
        return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * tt +
                (-p0 + 3f * p1 - 3f * p2 + p3) * ttt
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.color = pathColor;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, waypointGizmoRadius);
            UnityEditor.Handles.Label(
                waypoints[i].position + Vector3.up * 0.3f,
                $"WP {i}"
            );
        }

        int count = waypoints.Count;
        // How many segments to draw: all loops back to start if loopPath, otherwise stop before last
        int segmentCount = loopPath ? count : count - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            // Wrap all four control points around the waypoint list
            Vector3 p0 = waypoints[loopPath ? (i - 1 + count) % count : Mathf.Max(i - 1, 0)].position;
            Vector3 p1 = waypoints[i].position;
            Vector3 p2 = waypoints[(i + 1) % count].position;
            Vector3 p3 = waypoints[loopPath ? (i + 2) % count : Mathf.Min(i + 2, count - 1)].position;

            Vector3 prev = CatmullRom(p0, p1, p2, p3, 0f);
            for (int step = 1; step <= curveResolution; step++)
            {
                float t = step / (float)curveResolution;
                Vector3 next = CatmullRom(p0, p1, p2, p3, t);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
#endif 
}
