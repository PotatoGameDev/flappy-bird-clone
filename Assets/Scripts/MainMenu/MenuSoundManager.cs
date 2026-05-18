using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MenuSoundManager : MonoBehaviour
{
    private readonly WaitForSeconds WAIT_ONE_SEC = new(1);

    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip[] musicClips;

    [SerializeField] private bool playOnAwake;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioMixer mixer;

    void Awake()
    {
        if (playOnAwake)
        {
            StartCoroutine(PlayRandomMusic());
        }
    }

    void Start()
    {
        LoadVolume("MasterVolume");
        LoadVolume("MusicVolume");
        LoadVolume("SoundVolume");
    }

    public IEnumerator PlayRandomMusic()
    {
        int currentSong = Random.Range(0, musicClips.Length);
        while (true)
        {
            if (musicSource.isPlaying)
            {
                yield return WAIT_ONE_SEC;
            }
            else
            {
                musicSource.Stop();
                AudioClip clip = musicClips[currentSong % musicClips.Length];
                musicSource.clip = clip;
                musicSource.Play();

                yield return new WaitForSeconds(clip.length);
                currentSong++;
            }
        }
    }

    public void PlayClick()
    {
        sfxSource.PlayOneShot(clickClip);
    }

    public void PlayUpgrade()
    {
        sfxSource.PlayOneShot(upgradeClip);
    }

    public void PlaySelect()
    {
        if (sfxSource != null)
        {
            //TODO This is because onSelect happens somewhere before Awake happens, and npe:
            sfxSource.PlayOneShot(selectClip);
        }
    }

    // Volume control
    // All the functions here expect normalized, decibel values.

    public float LoadVolume(string group)
    {
        float volume = PlayerPrefs.GetFloat(group, 0f);
        SetVolume(group, volume);
        return volume;
    }

    public void SetAndSaveVolume(string group, float volume)
    {
        SetVolume(group, volume);
        PlayerPrefs.SetFloat(group, volume);
    }

    private void SetVolume(string group, float volume)
    {
        bool res = mixer.SetFloat(group, volume);
    }

}
