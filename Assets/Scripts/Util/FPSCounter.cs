using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    [SerializeField] float updateInterval = 0.5f;

    TextMeshProUGUI label;
    float timer;
    int frameCount;

    void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            int fps = Mathf.RoundToInt(frameCount / timer);
            label.text = $"{fps} FPS";
            frameCount = 0;
            timer = 0f;
        }
    }
}
