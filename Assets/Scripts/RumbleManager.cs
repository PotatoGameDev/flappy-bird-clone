using UnityEngine;

using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using System.Collections;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager Instance { get; private set; }

    private readonly Dictionary<RumbleSource, float> rumblesLeft = new();
    private readonly Dictionary<RumbleSource, float> rumblesRight = new();

    [SerializeField]
    private PlayerInput playerInput;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Instance " + Instance + " this: " + this);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;

        Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
    }

    void OnDisable()
    {
        Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
    }

    public void StartSustainedRumble(
            RumbleSource rumbleSource,
            float left,
            float right
            )
    {
        rumblesLeft[rumbleSource] = left;
        rumblesRight[rumbleSource] = right;

        RecalculateSustainedShake();
    }

    public void StopSustainedRumble(RumbleSource rumbleSource)
    {
        rumblesLeft[rumbleSource] = 0f;
        rumblesRight[rumbleSource] = 0f;

        RecalculateSustainedShake();
    }

    private void RecalculateSustainedShake()
    {
        float left = 0f;
        float right = 0f;

        foreach (RumbleSource source in Enum.GetValues(typeof(RumbleSource)))
        {
            if (rumblesRight.ContainsKey(source))
            {
                left += rumblesLeft?[source] ?? 0f;
                right += rumblesRight?[source] ?? 0f;
            }
        }

        if (playerInput.currentControlScheme == "Gamepad")
        {
            Gamepad.current?.SetMotorSpeeds(left, right);
        }
        else
        {
            Gamepad.current?.SetMotorSpeeds(0.0f, 0.0f);
        }
    }

    public void ImpulseRumble(RumbleSource source, float force, float time)
    {
        StartSustainedRumble(source, force, force);

        StartCoroutine(EndImpulseRumble(source, time));
    }

    private IEnumerator EndImpulseRumble(RumbleSource source, float time)
    {
        yield return new WaitForSeconds(time);

        StopSustainedRumble(source);
    }
}

public enum RumbleSource
{
    BoundaryDamage,
    PlasmaBeam,
    Jump,
    MenuSystem
}
