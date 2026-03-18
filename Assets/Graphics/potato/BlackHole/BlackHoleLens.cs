using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Attach to your Black Hole GameObject.
///
/// DeadzoneRadiusWorld should match the visible black disc in world units.
/// Since _HorizonRadius in the EventHorizon shader is in UV space (0–0.5),
/// the world radius = _HorizonRadius * objectScale.
/// e.g. HorizonRadius=0.08, scale=5 → DeadzoneRadiusWorld = 0.08 * 5 = 0.4
///
/// FadeRadiusWorld controls how far the warp reaches beyond the disc.
/// LensStrength and LensFalloff are tweaked in the Material Inspector.
///
/// </summary>
[ExecuteAlways]
public class BlackHoleLens : MonoBehaviour
{
    [Tooltip("Material using Custom/BlackHole/GravitationalLens")]
    public Material LensMaterial;

    [Header("World-Space Settings")]
    [Tooltip("Radius of the visible black disc in world units. " +
             "Formula: _HorizonRadius * this_object_scale. " +
             "e.g. HorizonRadius=0.08, scale=5 → 0.4")]
    public float DeadzoneRadiusWorld = 0.5f;

    [Tooltip("The warp fades to zero at this distance from the centre, in world units.")]
    public float FadeRadiusWorld = 3f;

    [Tooltip("Multiplier on DeadzoneRadiusWorld for a little inner padding.")]
    [Range(1f, 2f)]
    public float DeadzonePadding = 1.1f;

    static readonly int ID_Pos = Shader.PropertyToID("_BlackHolePos");
    static readonly int ID_Deadzone = Shader.PropertyToID("_DeadzoneRadius");
    static readonly int ID_Fade = Shader.PropertyToID("_FadeRadius");

    bool _subscribed;

    void Subscribe() { if (_subscribed) return; RenderPipelineManager.beginCameraRendering += OnBeginCamera; _subscribed = true; }
    void Unsubscribe() { if (!_subscribed) return; RenderPipelineManager.beginCameraRendering -= OnBeginCamera; _subscribed = false; }

    void OnEnable() { Subscribe(); }
    void Start() { Subscribe(); }
    void OnDisable() { Unsubscribe(); LensMaterial?.SetVector(ID_Pos, new Vector4(-9f, -9f, 0f, 0f)); }
    void OnDestroy() => Unsubscribe();

    void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
    {
        if (LensMaterial == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);

        if (vp.z < 0f)
        {
            LensMaterial.SetVector(ID_Pos, new Vector4(-9f, -9f, 0f, 0f));
            LensMaterial.SetFloat(ID_Deadzone, 0f);
            LensMaterial.SetFloat(ID_Fade, 0f);
            return;
        }

        LensMaterial.SetVector(ID_Pos, new Vector4(vp.x, vp.y, 0f, 0f));

        float deadzoneR = Mathf.Max(DeadzoneRadiusWorld * DeadzonePadding, 0.001f);
        float fadeR = Mathf.Max(FadeRadiusWorld, deadzoneR + 0.01f);

        LensMaterial.SetFloat(ID_Deadzone, WorldRadiusToViewport(cam, deadzoneR));
        LensMaterial.SetFloat(ID_Fade, WorldRadiusToViewport(cam, fadeR));
    }

    float WorldRadiusToViewport(Camera cam, float worldRadius)
    {
        if (cam.orthographic)
            return worldRadius / (cam.orthographicSize * 2f);

        Vector3 centre = cam.WorldToViewportPoint(transform.position);
        Vector3 edgeWorld = transform.position + cam.transform.right * worldRadius;
        Vector3 edge = cam.WorldToViewportPoint(edgeWorld);
        return Vector2.Distance(new Vector2(centre.x, centre.y), new Vector2(edge.x, edge.y));
    }

    // Helper shown in Inspector so you can verify the auto-calculated value
    [ContextMenu("Auto-calculate Deadzone from Renderer Scale")]
    void AutoCalculateDeadzone()
    {
        // _HorizonRadius default is 0.08 in UV space; world radius = UV radius * scale
        // The sprite UV goes -0.5 to 0.5, so full sprite width = 1 UV unit = lossyScale.x world units
        float horizonUV = 0.08f; // default _HorizonRadius — update if you changed it
        DeadzoneRadiusWorld = horizonUV * transform.lossyScale.x;
        Debug.Log($"[BlackHoleLens] Auto-set DeadzoneRadiusWorld = {DeadzoneRadiusWorld} " +
                  $"(horizonUV={horizonUV} * scale={transform.lossyScale.x}). " +
                  $"Update horizonUV in the script if you changed _HorizonRadius in the shader.");
    }
}
