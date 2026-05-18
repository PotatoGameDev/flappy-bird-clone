using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSliderController : MonoBehaviour
{
    [SerializeField] private string outputGroupParameterName;
    [SerializeField] private MenuSoundManager soundManager;
    [SerializeField] private Slider slider;

    void Awake()
    {
        float decibel = soundManager.LoadVolume(outputGroupParameterName);

        slider.value = DecibelToLinear(decibel);
    }

    public void SliderChanged()
    {
        float volume = LinearToDecibel(slider.value);

        Debug.Log("Setting " + outputGroupParameterName + " to " + volume);

        soundManager.SetAndSaveVolume(outputGroupParameterName, volume);
    }

    private static float LinearToDecibel(float linear)
    {
        if (linear > 0.0001f)
        {
            return Mathf.Log10(linear) * 20f;
        }
        else
        {
            return -80f;
        }
    }

    private static float DecibelToLinear(float decibel)
    {
        if (decibel > -80f)
        {
            return Mathf.Pow(10, decibel / 20f);
        }
        else
        {
            return 0f;
        }
    }
}
