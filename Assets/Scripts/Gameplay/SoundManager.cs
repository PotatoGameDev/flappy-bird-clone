using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public float clipDuration = 1.0f;
    public float fadeDuration = 0.5f;

    private bool screamsPlaying;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayScreams(AudioSource audioSource)
    {
        if (!audioSource.isPlaying)
        {
            StartCoroutine(PlaySliceCoroutine(audioSource));
        }
    }

    IEnumerator PlaySliceCoroutine(AudioSource source)
    {
        AudioClip clip = source.clip;

        float maxStartTime = clip.length - clipDuration;
        float randomStart = Random.Range(0f, maxStartTime);

        source.time = randomStart;
        source.volume = 0f;
        source.Play();

        // Fade in
        yield return FadeVolume(source, 0f, 1f, fadeDuration);

        // Play middle section
        yield return new WaitForSeconds(clipDuration - fadeDuration * 2f);

        // Fade out
        yield return FadeVolume(source, 1f, 0f, fadeDuration);

        source.Stop();
    }

    IEnumerator PlayFirstSliceCoroutine(AudioSource source)
    {
        source.volume = 0f;
        source.Play();

        // Fade in
        yield return FadeVolume(source, 0f, 1f, fadeDuration);

        // Play middle section
        yield return new WaitForSeconds(clipDuration - fadeDuration * 2f);

        // Fade out
        yield return FadeVolume(source, 1f, 0f, fadeDuration);

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

    public void PlayRandom(AudioSource source, AudioClip[] clips)
    {
        if (source.isPlaying) source.Stop();

        source.clip = clips[Random.Range(0, clips.Length)];

        StartCoroutine(PlayFirstSliceCoroutine(source));
    }
}
