using UnityEngine;
using System.Collections.Generic;
using System;

public class SwarmController : MonoBehaviour
{
    [SerializeField] internal List<SwarmFollow> Boids;
    [SerializeField] private Transform target;
    [SerializeField] private float neighborRadius = 2f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationWeight = 2f;

    [Header("Alignment")]
    [SerializeField] private float alignmentWeight = 1f;

    [Header("Cohesion")]
    [SerializeField] private float cohesionWeight = 1f;

    [Header("Follow")]
    [SerializeField] private float leaderWeight = 3f;

    [Header("Speed")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float maxSpeedDistance = 10f;

    [Header("Obstacles")]
    [SerializeField] private float obstacleRadius = 5f;
    [SerializeField] private float obstacleWeight = 1f;
    [SerializeField] private Transform[] obstacles;

    [Header("Smoothing")]
    [SerializeField] private float smoothingMax = 0.85f;

    public event Action SwarmBoidDied;

    private readonly Dictionary<SwarmFollow, Vector2> smoothedVelocities = new();

    private readonly List<Vector2> pendingPositions = new();

    void FixedUpdate()
    {
        if (Boids.Count == 0) return;

        int removed = Boids.RemoveAll(boid => boid == null || !boid.enabled || !boid.gameObject.activeInHierarchy);
        if (removed > 0)
            SwarmBoidDied?.Invoke();

        // --- Pass 1: compute all new positions, store them ---
        pendingPositions.Clear();
        foreach (SwarmFollow boid in Boids)
        {
            if (!boid.Active)
            {
                // Push a sentinel so indices stay aligned with Boids list
                pendingPositions.Add(boid.Position);
                continue;
            }

            Vector2 steering = Vector2.zero;

            Vector2 toTarget = (Vector2)target.position - boid.Position;
            float distToTarget = toTarget.magnitude;
            steering += toTarget.normalized * leaderWeight;

            List<SwarmFollow> neighbors = GetNeighbors(boid);
            if (neighbors.Count > 0)
            {
                steering += Separation(boid, neighbors) * separationWeight;
                steering += Alignment(neighbors) * alignmentWeight;
                steering += Cohesion(boid, neighbors) * cohesionWeight;
            }

            steering += SeparationObstacle(boid, obstacles) * obstacleWeight;

            // Speed ramp: 0 at target, maxSpeed at maxSpeedDistance
            float speed = Mathf.Lerp(minSpeed, maxSpeed, Mathf.Clamp01(distToTarget / maxSpeedDistance));

            Vector2 desiredVelocity = steering.normalized * speed;

            // Smoothing: more smoothing when close (avoids jitter), less when far
            float smoothing = Mathf.Lerp(smoothingMax, 0f, Mathf.Clamp01(distToTarget / maxSpeedDistance));

            if (!smoothedVelocities.TryGetValue(boid, out Vector2 smoothed))
                smoothed = desiredVelocity;

            // Isolated and far: reset stale momentum so boid can turn around
            if (neighbors.Count == 0 && distToTarget > maxSpeedDistance)
                smoothed = desiredVelocity;
            else
                smoothed = Vector2.Lerp(desiredVelocity, smoothed, smoothing);

            smoothedVelocities[boid] = smoothed;

            pendingPositions.Add(boid.Position + smoothed * Time.fixedDeltaTime);
        }

        // --- Pass 2: apply all positions at once ---
        for (int i = 0; i < Boids.Count; i++)
        {
            if (!Boids[i].Active)
            {
                continue;
            }
            Boids[i].MovePosition(pendingPositions[i]);
        }
    }

    private List<SwarmFollow> GetNeighbors(SwarmFollow boid)
    {
        List<SwarmFollow> neighbors = new();
        foreach (SwarmFollow n in Boids)
        {
            if (boid == n) continue;
            if (!n.Active) continue;  // was checking boid.Active instead of n.Active - bug fix
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
                Vector2 away = (boid.Position - n.Position).normalized;
                steer += away * (1f - dist / separationRadius);
                count++;
            }
        }
        if (count > 0) steer /= count;
        return steer;
    }

    private Vector2 SeparationObstacle(SwarmFollow boid, Transform[] obstacles)
    {
        Vector2 steer = Vector2.zero;
        int count = 0;
        foreach (Transform n in obstacles)
        {
            if (n == null) continue;
            float dist = Vector2.Distance(boid.Position, n.position);
            if (dist < obstacleRadius && dist > 0.0001f)
            {
                Vector2 away = (boid.Position - (Vector2)n.position).normalized;
                steer += away * (1f - dist / obstacleRadius);
                count++;
            }
        }
        if (count > 0) steer /= count;
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
