using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public float clipDuration = 1.0f;
    public float fadeDuration = 0.5f;

    [SerializeField]
    private AudioSource quakeAudioSource;

    [SerializeField]
    private AudioSource hitAudioSource;

    [SerializeField]
    private AudioSource screamAudioSource;

    [SerializeField]
    private AudioSource collectAudioSource;

    [SerializeField]
    private AudioSource explosionAudioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayScreams(float volume = 1f)
    {
        if (!screamAudioSource.isPlaying)
        {
            StartCoroutine(PlaySliceCoroutine(screamAudioSource, volume));
        }
    }

    public void PlayCollect(float pitch)
    {
        collectAudioSource.pitch = pitch;
        collectAudioSource.Play();
    }

    IEnumerator PlaySliceCoroutine(AudioSource source, float volume = 1f)
    {
        AudioClip clip = source.clip;

        float maxStartTime = clip.length - clipDuration;
        float randomStart = Random.Range(0f, maxStartTime);

        source.time = randomStart;
        source.volume = 0f;
        source.Play();

        // Fade in
        yield return FadeVolume(source, 0f, volume, fadeDuration);

        // Play middle section
        yield return new WaitForSeconds(clipDuration - fadeDuration * 2f);

        // Fade out
        yield return FadeVolume(source, volume, 0f, fadeDuration);

        source.Stop();
    }

    IEnumerator PlayFirstSliceCoroutine(AudioSource source, float volume = 1f)
    {
        source.volume = 0f;
        source.time = 0f;
        source.Play();

        // Fade in
        yield return FadeVolume(source, 0f, volume, fadeDuration);

        // Play middle section
        yield return new WaitForSeconds(clipDuration - fadeDuration * 2f);

        // Fade out
        yield return FadeVolume(source, volume, 0f, fadeDuration);

        source.Stop();
    }


    IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        source.volume = to;
    }

    public void PlayRandomQuake(AudioClip[] clips, float volume)
    {
        PlayRandom(quakeAudioSource, clips, volume);
    }

    public void PlayRandomHit(AudioClip[] clips, float volume)
    {
        PlayRandom(hitAudioSource, clips, volume);
    }

    public void PlayExplosion()
    {
        explosionAudioSource.PlayOneShot(explosionAudioSource.clip);
    }

    private void PlayRandom(AudioSource source, AudioClip[] clips, float volume)
    {
        if (source.isPlaying) source.Stop();

        source.clip = clips[Random.Range(0, clips.Length)];

        StartCoroutine(PlayFirstSliceCoroutine(source, volume));
    }
}
