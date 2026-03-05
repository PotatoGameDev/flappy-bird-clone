using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FadingTextController : MonoBehaviour
{
    public float fallingSpeed;
    public float fadeDuration;

    private TextMeshProUGUI label;

    void Awake()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator FadeOut()
    {
        Color startColor = label.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0, time / fadeDuration);
            label.color = new(startColor.r, startColor.g, startColor.b, alpha);

            transform.Translate(new(0f, -fallingSpeed * Time.deltaTime, 0f));

            yield return null;
        }

        Destroy(gameObject);
    }

    public void Init(string text)
    {
        label.text = text;
        StartCoroutine(FadeOut());
    }
}
