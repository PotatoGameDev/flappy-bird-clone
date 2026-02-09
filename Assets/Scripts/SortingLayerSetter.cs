using UnityEngine;

public class SortingLayerSetter : MonoBehaviour
{

    public string sortingLayerName = "Default";
    public int orderInLayer = 0;

    void Awake()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer rendr in renderers)
        {
            rendr.sortingLayerName = sortingLayerName;
            rendr.sortingOrder = orderInLayer;
        }
        // We only need this script on startup
        Destroy(this);
    }

}
