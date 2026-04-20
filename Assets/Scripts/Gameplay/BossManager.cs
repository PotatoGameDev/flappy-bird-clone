using UnityEngine;
using TMPro;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject flyingSaucersBossContainer;
    [SerializeField] private float flyingSaucersBossPlayerSpeed = 10.0f;

    [SerializeField] private GameObject bossNameTag;
    [SerializeField] private TextMeshProUGUI bossNameTagLabel;

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
        GameplayManager.Instance.Player.speed = flyingSaucersBossPlayerSpeed;

        float newX = GameplayManager.Instance.Player.transform.position.x
            + flyingSaucersBossContainerOffset;
        Vector3 newPos = flyingSaucersBossContainer.transform.position;
        newPos.x = newX;

        flyingSaucersBossContainer.transform.position = newPos;
        flyingSaucersBossContainer.SetActive(true);

        bossNameTag.SetActive(true);
        bossNameTagLabel.SetText("Irritating Motherships" + " Attacks");
        StartCoroutine(DestroyBossNameTag());

        SoundManager.Instance.PlayBossMusic();
    }

    private IEnumerator DestroyBossNameTag()
    {
        yield return new WaitForSeconds(2f);
        Destroy(bossNameTag);
    }
}
