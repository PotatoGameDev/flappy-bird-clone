using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject flyingSaucersBossContainer;

    void Start()
    {
        if (GameManager.Instance.levelSettings.levelType == LevelType.BossFight)
        {
            switch (GameManager.Instance.CurrentLevel)
            {
                case 1:
                    flyingSaucersBossContainer.SetActive(true);
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    Debug.LogWarning("Level not supported");
                    break;
            }
        }
    }

}
