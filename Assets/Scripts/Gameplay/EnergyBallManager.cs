using UnityEngine;
using System.Collections.Generic;

public class EnergyBallManager : MonoBehaviour
{
    public static EnergyBallManager Instance { get; private set; }

    [SerializeField] private Color whiteColor;
    [SerializeField] private Color yellowColor;
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    public EnergyBallController GetRandom(int gate)
    {
        float white = 100f;
        float yellow = Mathf.Max(0f, Mathf.Pow(Mathf.Max(0, gate - 10), 1.5f) * 0.3f);
        float red = Mathf.Max(0f, Mathf.Pow(Mathf.Max(0, gate - 30), 1.5f) * 0.1f);
        float blue = Mathf.Max(0f, Mathf.Pow(Mathf.Max(0, gate - 60), 1.5f) * 0.03f);

        float total = white + yellow + red + blue;
        float roll = Random.Range(0f, total);

        if (roll < white) return GetWhite();
        if (roll < white + yellow) return GetYellow();
        if (roll < white + yellow + red) return GetRed();
        return GetBlue();
    }

    public Stack<EnergyBallController> GetForTotal(int total)
    {
        Stack<EnergyBallController> result = new();
        int current = total;
        while (current > 0)
        {
            if (current >= 1000)
            {
                current -= 1000;
                result.Push(GetBlue());
            }
            else if (current >= 100)
            {
                current -= 100;
                result.Push(GetRed());
            }
            else if (current >= 10)
            {
                current -= 10;
                result.Push(GetYellow());
            }
            else if (current >= 1)
            {
                current -= 1;
                result.Push(GetWhite());
            }
        }

        return result;
    }

    public EnergyBallController GetWhite()
    {
        return Create(whiteColor, 1);
    }

    public EnergyBallController GetYellow()
    {
        return Create(yellowColor, 10);
    }

    public EnergyBallController GetRed()
    {
        return Create(redColor, 100);
    }

    public EnergyBallController GetBlue()
    {
        return Create(blueColor, 1000);
    }

    private EnergyBallController Create(Color color, int value)
    {
        EnergyBallController ball = InstancePoolsManager.Instance.EnergyBallControllerPool.Get();
        ball.SetColor(color, value);
        return ball;
    }
}
