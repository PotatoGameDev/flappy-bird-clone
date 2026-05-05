using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PerformanceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Init()
    {
#if UNITY_ANDROID
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.RoundToInt(
                (float)Screen.currentResolution.refreshRate
                );

        Screen.SetResolution(
                Mathf.RoundToInt(Screen.width * 0.75f),
                Mathf.RoundToInt(Screen.height * 0.75f),
                true
                );
#endif
    }
}
