using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public GameObject[] backgroundImages;
    public float imageWidth;

    void Awake()
    {
        foreach (GameObject im in backgroundImages)
        {
            im.SetActive(false);
        }

        // We activate a random one:
        //
        GameObject randomImage = backgroundImages[Random.Range(0, backgroundImages.Length)];
        randomImage.SetActive(true);

        SpriteRenderer sr = randomImage.GetComponent<SpriteRenderer>();
        imageWidth = sr.bounds.size.x;
    }
}
