using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityBeeper : MonoBehaviour
{
    [Header("References")]
    public AudioClip beepClip;

    [Header("Distance Mapping")]
    public float maxDistance = 50f;
    public float minDistance = 2f;
    public float maxInterval = 1.2f;
    public float minInterval = 0.1f;

    private AudioSource audioSource;
    private float timer;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = beepClip;
        audioSource.loop = false; // we'll trigger it manually, not use built-in loop
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        PlanetController player = GameplayManager.Instance.Player;
        float distance = Vector3.Distance(transform.position, player.transform.position);

        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float interval = Mathf.Lerp(minInterval, maxInterval, t);

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            audioSource.PlayOneShot(beepClip);
            timer = interval;
        }
    }
}
