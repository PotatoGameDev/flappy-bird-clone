using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 0.1f;
    public float GetDelaySeconds() => delaySeconds;

    public Vector3 PlayerOffset { get; private set; }

    private bool jumpedThisFrame;
    private float currentJumpForce = 0f;

    public static Queue<JumpLog> jumpQueue = new();

    private Rigidbody2D rb;

    private FinalBossPhase phase = FinalBossPhase.Init;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        PlanetController player = GameplayManager.Instance.Player;
        Vector3 position = player.transform.position;
        position.x += player.speed * delaySeconds;
        transform.position = position;
        PlayerOffset = transform.position - player.transform.position;

        StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        yield return new WaitForSeconds(delaySeconds);

        phase = FinalBossPhase.Chase;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void Update()
    {
        float jumpForce = 0f;
        if (phase != FinalBossPhase.Init)
        {
            if (jumpQueue.Count > 0)
            {
                var entry = jumpQueue.Peek();

                if (Time.time - entry.timestamp >= delaySeconds)
                {
                    jumpQueue.Dequeue();

                    if (entry.didJump)
                    {
                        jumpForce = entry.force;
                    }
                }
            }
        }

        float effectiveSpeed = GameplayManager.Instance.Player.speed;

        // See PlanetController, the same logic:
        if (jumpForce > 0f)
        {
            rb.linearVelocity = new Vector2(effectiveSpeed, jumpForce);
        }
        else
        {
            rb.linearVelocity = new Vector2(effectiveSpeed, rb.linearVelocity.y);
        }
    }

    public void RegisterJump(float force)
    {
        currentJumpForce = force;
        jumpedThisFrame = true;
    }

    void LateUpdate()
    {
        if (phase == FinalBossPhase.Init)
        {
            return;
        }
        if (jumpedThisFrame)
        {
            jumpQueue.Enqueue(new JumpLog
            {
                timestamp = Time.time,
                didJump = jumpedThisFrame,
                force = currentJumpForce,
            });

            // Resetting
            jumpedThisFrame = false;
            currentJumpForce = 0f;
        }
    }
}

public struct JumpLog
{
    public float timestamp;
    public bool didJump;
    public float force;
}

enum FinalBossPhase
{
    Init, Chase
}

