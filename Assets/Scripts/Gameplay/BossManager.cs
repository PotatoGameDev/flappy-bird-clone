using UnityEngine;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject flyingSaucersBossContainer;

    private bool bossActive;

    private float flyingSaucersBossContainerOffset;

    void Start()
    {
        flyingSaucersBossContainer.SetActive(false);

        flyingSaucersBossContainerOffset = (
                flyingSaucersBossContainer.transform.position -
                GameplayManager.Instance.Player.transform.position
            ).x;
    }

    public bool IsBossActive()
    {
        return bossActive;
    }

    public void ActivateBoss()
    {
        bossActive = true;

        float newX = GameplayManager.Instance.Player.transform.position.x
            + flyingSaucersBossContainerOffset;
        Vector3 newPos = flyingSaucersBossContainer.transform.position;
        newPos.x = newX;

        flyingSaucersBossContainer.transform.position = newPos;
        flyingSaucersBossContainer.SetActive(true);
    }

}
