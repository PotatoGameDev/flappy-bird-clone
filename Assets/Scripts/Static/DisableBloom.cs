using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DisableBloom : MonoBehaviour
{
    void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (FindVolumeComponent<Bloom>(out var bloom))
            bloom.active = false;
#endif
    }

    static bool FindVolumeComponent<T>(out T component) where T : VolumeComponent
    {
        component = null;
        var volume = Object.FindObjectOfType<Volume>();
        return volume != null && volume.profile.TryGet(out component);
    }
}
