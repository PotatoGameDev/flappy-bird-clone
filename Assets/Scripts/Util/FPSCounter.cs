using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    float deltaTime;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int fps = Mathf.RoundToInt(1.0f / deltaTime);
        GUI.Label(new Rect(10, 10, 100, 25), $"{fps} FPS");
    }
}
