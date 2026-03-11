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

    IEnumerator Fall(RectTransform rect)
    {
        Vector2 start = rect.anchoredPosition + new Vector2(0, 50);
        Vector2 end = rect.anchoredPosition;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }
    }

    public void Init(string text)
    {
        label.text = text;
        StartCoroutine(FadeOut());
    }

    public void Init2(string text, RectTransform rect)
    {
        label.text = text;
        StartCoroutine(Fall(rect));
    }
}
