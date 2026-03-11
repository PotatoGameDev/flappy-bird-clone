using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class FadingMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float maxDisplayTimeSeconds = 1;

    private RectTransform rect;

    private float displayTime;
    private Vector2 targetPosition;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        displayTime = 0f;
    }

    public void SetColor(Color color)
    {
        label.color = color;
    }

    public void SetPosition(Vector2 pos)
    {
        rect.anchoredPosition = pos;
    }

    public void SetText(string value)
    {
        label.text = value;
    }

    public void SetTarget(Vector2 pos)
    {
        targetPosition = pos;
    }

    void Update()
    {
        rect.anchoredPosition = Vector2.Lerp(
            rect.anchoredPosition,
            targetPosition,
            Time.deltaTime * 5f
        );

        if (displayTime > maxDisplayTimeSeconds)
        {
            Color color = label.color;
            color.a = 0f;
            label.color = Color.Lerp(label.color, color, Time.deltaTime * 10f);
        }
        displayTime += Time.deltaTime;
    }
}
