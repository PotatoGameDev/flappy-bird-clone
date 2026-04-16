using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuSoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip selectClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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
