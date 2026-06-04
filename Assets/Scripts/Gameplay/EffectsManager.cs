using UnityEngine;
using Unity.Cinemachine;

using System.Collections.Generic;
using System;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float baseImpulseForce;

    private readonly Dictionary<ShakeSource, float> sustainedShakesAmplitudes = new();
    private readonly Dictionary<ShakeSource, float> sustainedShakesFrequencies = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Instance " + Instance + " this: " + this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public void Shake()
    {
        impulseSource.GenerateImpulse(baseImpulseForce);
    }

    public void Shake(Vector2 direction)
    {
        direction.Normalize();
        impulseSource.GenerateImpulseWithVelocity(baseImpulseForce * direction);
    }

    public void StartSustainedShake(ShakeSource shakeSource, float amplitude = 1f, float frequency = 1f)
    {
        sustainedShakesAmplitudes[shakeSource] = amplitude;
        sustainedShakesFrequencies[shakeSource] = frequency;

        RecalculateSustainedShake();
    }

    public void StopSustainedShake(ShakeSource shakeSource)
    {
        sustainedShakesAmplitudes[shakeSource] = 0f;
        sustainedShakesFrequencies[shakeSource] = 0f;

        RecalculateSustainedShake();
    }

    private void RecalculateSustainedShake()
    {
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
        foreach (ShakeSource source in Enum.GetValues(typeof(ShakeSource)))
        {
            if (sustainedShakesFrequencies.ContainsKey(source))
            {
                noise.AmplitudeGain += sustainedShakesAmplitudes?[source] ?? 0f;
                noise.FrequencyGain += sustainedShakesFrequencies?[source] ?? 0f;
            }
        }

    }
}

public enum ShakeSource
{
    BoundaryDamage,
    PlasmaBeam
}
