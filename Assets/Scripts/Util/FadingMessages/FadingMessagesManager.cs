using UnityEngine;
using System.Collections.Generic;
using PotatoGameDev.Pool;

public class FadingMessagesManager : MonoBehaviour
{
    [SerializeField] private FadingMessage messagePrefab;
    private InstancePool<FadingMessage> fadingMessagePool;

    [SerializeField] private RectTransform container;

    [SerializeField] private float spacing = 10f;

    private readonly List<FadingMessage> labels = new();


    void Awake()
    {
        fadingMessagePool = new(messagePrefab, container);
    }

    public void Spawn(string text, Color color)
    {
        FadingMessage label = fadingMessagePool.Get();

        label.SetColor(color);
        label.SetPosition(new Vector2(0, 50));
        label.SetText(text);
        labels.Insert(0, label);

        UpdatePositions();
    }

    private void UpdatePositions()
    {
        labels.RemoveAll(l => l == null);
        for (int i = 0; i < labels.Count; i++)
        {
            Vector2 pos = new(0, -i * spacing);
            labels[i].SetTarget(pos);
        }
    }
}
