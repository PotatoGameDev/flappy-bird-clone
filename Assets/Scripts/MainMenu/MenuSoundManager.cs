using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MenuSoundManager : MonoBehaviour
{
    private readonly WaitForSeconds WAIT_ONE_SEC = new(1);

    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip[] musicClips;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayRandomMusic());
    }

    public IEnumerator PlayRandomMusic()
    {
        int currentSong = Random.Range(0, musicClips.Length);
        while (true)
        {
            if (audioSource.isPlaying)
            {
                yield return WAIT_ONE_SEC;
            }
            else
            {
                audioSource.clip = musicClips[currentSong % musicClips.Length];
                audioSource.Play();

                yield return new WaitForSeconds(audioSource.clip.length);
                currentSong++;
            }
        }
    }

    public void PlayClick()
    {
        audioSource.PlayOneShot(clickClip);
    }

    public void PlayUpgrade()
    {
        audioSource.PlayOneShot(upgradeClip);
    }

    public void PlaySelect()
    {
        if (audioSource != null)
        {
            //TODO This is because onSelect happens somewhere before Awake happens, and npe:
            audioSource.PlayOneShot(selectClip);
        }
    }
}
