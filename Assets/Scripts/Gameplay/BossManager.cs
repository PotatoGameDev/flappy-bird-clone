using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject flyingSaucersBossContainer;


    [SerializeField] private bool forceBoss;

    void Start()
    {
        LevelType levelType = GameManager.Instance.levelSettings.levelType;
        int currentLevel = GameManager.Instance.CurrentLevel;

        flyingSaucersBossContainer.SetActive(forceBoss || currentLevel == 0 && levelType == LevelType.BossFight);
        // TODO Next levels
    }

}
