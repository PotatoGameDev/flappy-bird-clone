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
    }

    public void StartGame()
    {
        State = SaveSystem.Load();
    }

    public void Save()
    {
        SaveSystem.Save(State);
    }

}

[System.Serializable]
public class GameState
{
    public int Population;
    public int CivType;
}
