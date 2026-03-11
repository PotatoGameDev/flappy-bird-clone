using UnityEngine;
using System.Collections.Generic;

public class FadingMessagesManager : MonoBehaviour
{
    [SerializeField] private FadingMessage messagePrefab;
    [SerializeField] private RectTransform container;

    [SerializeField] private float spacing = 10f;

    private readonly List<FadingMessage> labels = new();

    public void Spawn(string text, Color color)
    {
        FadingMessage label = Instantiate(messagePrefab, container);

        label.SetColor(color);
        label.SetPosition(new Vector2(0, 50));
        label.SetText(text);
        labels.Insert(0, label);

        UpdatePositions();
        Destroy(label.gameObject, 3f);
    }

    private void UpdatePositions()
    {
        for (int i = 0; i < labels.Count; i++)
        {
            Vector2 pos = new(0, -i * spacing);
            labels[i].SetTarget(pos);
        }
    }
}
