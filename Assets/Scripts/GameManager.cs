using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public PlanetController Player { get; set; }

    public GameState State = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        State.Population = 1000000;
        State.CivType = 1;
    }


    public void GameOver()
    {
        // TODO: Sumfin
    }

}

public class GameState
{
    public int Population { get; set; }
    public int CivType { get; set; }
}
