using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FadingTextController : MonoBehaviour
{
    public float fallingSpeed;
    public float fadeDuration;

    private TextMeshProUGUI label;

    private string[] infoTexts = {
        "{0} died",
        "{0} killed",
        "{0} squashed",
        "{0} evaporated",
        "{0} lost",
        "{0} are no more",
        "{0} are now ex-people",
        "{0} are poorly",
        "{0} need some milk",
        "{0} have a bad feeling about this",
        "{0} did redeem, ma'am",
        "{0} have no fun",
        "{0} perished",
        "{0} don't get no respect",
    };

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

    public void Init(float peopleDied)
    {
        string text = infoTexts[Random.Range(0, infoTexts.Length - 1)];
        label.text = string.Format(text, peopleDied);
        StartCoroutine(FadeOut());
    }
}
