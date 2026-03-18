using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlackHoleEffect : MonoBehaviour
{
    public float strength = 1.5f;
    public float radius = 3.0f;

    // Assign this material in Inspector or create it
    [SerializeField] private Material blackHoleMaterial;

    void Update()
    {
        if (!blackHoleMaterial) return;

        // Update distortion center to match black hole's screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        float centerX = screenPos.x / Screen.width;
        float centerY = screenPos.y / Screen.height;

        blackHoleMaterial.SetFloat("_Intensity", strength);
        blackHoleMaterial.SetFloat("_Radius", radius * 0.5f); // adjust for visual feel
        blackHoleMaterial.SetFloat("_CenterX", centerX);
        blackHoleMaterial.SetFloat("_CenterY", centerY);
    }
}

