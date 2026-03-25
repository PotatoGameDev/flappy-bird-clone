using UnityEngine;
using System.Collections.Generic;

public class SwarmController : MonoBehaviour
{
    [SerializeField] private SwarmFollow[] boids;
    [SerializeField] private Transform target;
    [SerializeField] private float neighborRadius = 2f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationWeight = 2f;

    [Header("Alignment")]
    [SerializeField] private float alignmentWeight = 1f;


    [Header("Cohesion")]
    [SerializeField] private float cohesionWeight = 1f;

    [Header("Noise")]
    [SerializeField] private float offsetNoiseSpeed = 0f;
    [SerializeField] private float offsetNoiseAmount = 0f;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float leaderWeight = 3f;

    [Header("Obstacles")]
    [SerializeField] private float obstacleRadius = 5f;
    [SerializeField] private float obstacleWeight = 1f;

    [SerializeField] private Transform[] obstacles;


    [Header("Border Avoidance")]
    [SerializeField] private Transform topBorder;
    [SerializeField] private Transform bottomBorder;
    [SerializeField] private Transform leftBorder;
    [SerializeField] private Transform rightBorder;

    [SerializeField] private float borderDistance = 2f;
    [SerializeField] private float borderWeight = 1f;


    [Header("Smoothing")]
    // Smoothing factor: 0 = no smoothing, 1 = never moves.
    [SerializeField] private float steeringSmoothing = 0.85f;

    private readonly Dictionary<SwarmFollow, Vector2> _smoothedVelocities = new();

    void FixedUpdate()
    {
        Debug.Assert(boids.Length > 0, "Add SwarmFollow children");

        foreach (SwarmFollow boid in boids)
        {
            // --- Accumulate steering forces as *desired directions*, not displacements ---
            Vector2 steering = Vector2.zero;

            // Leader: pull toward target
            Vector2 toTarget = ((Vector2)target.position - boid.Position).normalized;
            steering += toTarget * leaderWeight;

            // Flocking
            List<SwarmFollow> neighbors = GetNeighbors(boid);
            if (neighbors.Count > 0)
            {
                steering += Separation(boid, neighbors) * separationWeight;
                steering += Alignment(neighbors) * alignmentWeight;
                steering += Cohesion(boid, neighbors) * cohesionWeight;

                steering += SeparationObstacle(boid, obstacles) * obstacleWeight;

                steering += SeparationBorder(boid, topBorder, true) * borderWeight;
                steering += SeparationBorder(boid, bottomBorder, true) * borderWeight;
                steering += SeparationBorder(boid, leftBorder, false) * borderWeight;
                steering += SeparationBorder(boid, rightBorder, false) * borderWeight;
            }

            // Noise
            if (offsetNoiseAmount > 0f)
            {
                float nx = (Mathf.PerlinNoise(boid.Position.x + Time.time * offsetNoiseSpeed, 0f) - 0.5f) * offsetNoiseAmount;
                float ny = (Mathf.PerlinNoise(0f, boid.Position.y + Time.time * offsetNoiseSpeed) - 0.5f) * offsetNoiseAmount;
                steering += new Vector2(nx, ny);
            }

            // Convert steering direction into a desired velocity
            Vector2 desiredVelocity = steering.normalized * followSpeed;

            // Smooth the velocity over time to prevent jitter
            if (!_smoothedVelocities.TryGetValue(boid, out Vector2 smoothed))
                smoothed = desiredVelocity;

            smoothed = Vector2.Lerp(desiredVelocity, smoothed, steeringSmoothing);
            _smoothedVelocities[boid] = smoothed;

            // Move by the smoothed velocity
            Vector2 newPosition = boid.Position + smoothed * Time.fixedDeltaTime;
            //boid.MovePositionDirect(newPosition);
            boid.MovePositionDirect(newPosition);
        }
    }

    private List<SwarmFollow> GetNeighbors(SwarmFollow boid)
    {
        List<SwarmFollow> neighbors = new();
        foreach (SwarmFollow n in boids)
        {
            if (boid == n) continue;
            if (boid.Distance(n) < neighborRadius)
                neighbors.Add(n);
        }
        return neighbors;
    }

    private Vector2 Separation(SwarmFollow boid, List<SwarmFollow> neighbors)
    {
        Vector2 steer = Vector2.zero;
        int count = 0;
        foreach (SwarmFollow n in neighbors)
        {
            float dist = boid.Distance(n);
            if (dist < separationRadius && dist > 0.0001f)
            {
                // Normalize so a single close neighbor can't dominate
                Vector2 away = (boid.Position - n.Position).normalized;
                steer += away * (1f - dist / separationRadius); // stronger when closer
                count++;
            }
        }
        if (count > 0) steer /= count;
        return steer;
    }

    // Allows to add separation from a single obstacle, like avoiding a static element or the player.
    private Vector2 SeparationObstacle(SwarmFollow boid, Transform[] obstacles)
    {
        Vector2 steer = Vector2.zero;
        int count = 0;
        foreach (Transform n in obstacles)
        {
            if (n == null)
            {
                continue;
            }
            float dist = Vector2.Distance(boid.Position, n.position);
            if (dist < obstacleRadius && dist > 0.0001f)
            {
                // Normalize so a single close neighbor can't dominate
                Vector2 away = (boid.Position - (Vector2)n.position).normalized;
                steer += away * (1f - dist / obstacleRadius); // stronger when closer
                count++;
            }
        }
        if (count > 0) steer /= count;
        return steer;
    }

    // Allows to add separation from custom borders
    // This does not check on which side the boid is, it will just avoid that line, one way or the other.
    private Vector2 SeparationBorder(SwarmFollow boid, Transform border, bool horizontal)
    {
        if (border == null)
        {
            return Vector2.zero;
        }

        Vector2 steer = Vector2.zero;

        float dist;
        if (horizontal)
        {
            dist = Mathf.Abs(boid.Position.y - border.position.y);
        }
        else
        {
            dist = Mathf.Abs(boid.Position.x - border.position.x);
        }

        if (dist < borderDistance && dist > 0.0001f)
        {
            Vector2 away;
            if (horizontal)
            {
                away = new Vector2(0f, boid.Position.y - border.position.y).normalized;
            }
            else
            {
                away = new Vector2(boid.Position.y - border.position.y, 0f).normalized;
            }
            steer += away * (1f - dist / borderDistance); // stronger when closer
        }

        return steer;
    }

    private Vector2 Alignment(List<SwarmFollow> neighbors)
    {
        Vector2 avgVelocity = Vector2.zero;
        foreach (SwarmFollow n in neighbors)
            avgVelocity += n.LinearVelocity;
        avgVelocity /= neighbors.Count;
        return avgVelocity.sqrMagnitude > 0.0001f ? avgVelocity.normalized : Vector2.zero;
    }

    private Vector2 Cohesion(SwarmFollow boid, List<SwarmFollow> neighbors)
    {
        Vector2 avgPosition = Vector2.zero;
        foreach (SwarmFollow n in neighbors)
            avgPosition += n.Position;
        avgPosition /= neighbors.Count;
        return (avgPosition - boid.Position).normalized;
    }
}
