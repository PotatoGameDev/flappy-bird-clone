using UnityEngine;

public class MobileShaderBakeIn : MonoBehaviour
{
    [SerializeField] private SpriteRenderer shaderHolder;
    [SerializeField] private SpriteRenderer bakedSprite;
    void Start()
    {
#if UNITY_ANDROID
        shaderHolder.enabled = false;
        bakedSprite.enabled = true;
#else
        shaderHolder.enabled = true;
        bakedSprite.enabled = false;
#endif
    }
}
