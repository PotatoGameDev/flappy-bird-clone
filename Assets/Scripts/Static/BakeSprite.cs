using UnityEngine;

public class BakeSprite : MonoBehaviour
{
    public RenderTexture rt;
    public string fileName = "SunStarBaked.png";
    public string savePath = "Assets/Graphics/";

    [ContextMenu("Bake Now")]
    void Bake()
    {
        Texture2D tex = new(rt.width, rt.height, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        System.IO.File.WriteAllBytes(savePath + fileName, tex.EncodeToPNG());
        Debug.Log($"Saved to {savePath + fileName}");
    }
}
