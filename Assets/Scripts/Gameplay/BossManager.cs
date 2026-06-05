using UnityEngine;
using TMPro;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject flyingSaucersBossContainer;
    [SerializeField] private float flyingSaucersBossPlayerSpeed = 10.0f;

    [SerializeField] private GameObject bossNameTag;
    [SerializeField] private TextMeshProUGUI bossNameTagLabel;


    [SerializeField] private GameObject motherShipperBossContainer;
    [SerializeField] private float motherShipperBossPlayerSpeed = 10.0f;

    private bool bossActive;

    private float flyingSaucersBossContainerOffset;
    private float motherShipperBossContainerOffset;

    void Start()
    {
        flyingSaucersBossContainer.SetActive(false);

        flyingSaucersBossContainerOffset = (
                flyingSaucersBossContainer.transform.position -
                GameplayManager.Instance.Player.transform.position
            ).x;

        motherShipperBossContainer.SetActive(false);
        motherShipperBossContainerOffset = (
                motherShipperBossContainer.transform.position -
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

        int level = GameManager.Instance.CurrentLevel + 1;

        switch (level)
        {
            case 1:
                {
                    GameplayManager.Instance.Player.speed = flyingSaucersBossPlayerSpeed;

                    float newX = GameplayManager.Instance.Player.transform.position.x
                        + flyingSaucersBossContainerOffset;
                    Vector3 newPos = flyingSaucersBossContainer.transform.position;
                    newPos.x = newX;

                    flyingSaucersBossContainer.transform.position = newPos;
                    flyingSaucersBossContainer.SetActive(true);
                    break;
                }
            case 2:
                {
                    GameplayManager.Instance.Player.speed = motherShipperBossPlayerSpeed;

                    float newX = GameplayManager.Instance.Player.transform.position.x
                        + motherShipperBossContainerOffset;
                    Vector3 newPos = motherShipperBossContainer.transform.position;
                    newPos.x = newX;

                    motherShipperBossContainer.transform.position = newPos;
                    motherShipperBossContainer.SetActive(true);
                    break;
                }
            default:
                break;
        }

        bossNameTag.SetActive(true);
        bossNameTagLabel.SetText(Loc.Get("gameplay_boss_intro_title_" + level.ToString()));
        StartCoroutine(DestroyBossNameTag());
        SoundManager.Instance.PlayBossMusic();
    }

    private IEnumerator DestroyBossNameTag()
    {
        yield return new WaitForSeconds(2f);
        Destroy(bossNameTag);
    }
}
