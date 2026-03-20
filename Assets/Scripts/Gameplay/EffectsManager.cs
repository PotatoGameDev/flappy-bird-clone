using UnityEngine;
using Unity.Cinemachine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float baseImpulseForce;

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

    public void StartSustainedShake(float amplitude = 1f, float frequency = 1f)
    {
        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;
    }

    public void StopSustainedShake()
    {
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }

}
