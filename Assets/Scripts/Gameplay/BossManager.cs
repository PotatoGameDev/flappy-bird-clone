using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossManager : MonoBehaviour
{
    [SerializeField] private GameObject bossNameTag;
    [SerializeField] private TextMeshProUGUI bossNameTagLabel;
    [SerializeField] private TextMeshProUGUI bossNameLabel;
    private bool bossActive;

    // Boss 1
    [SerializeField] private GameObject flyingSaucersBossContainer;
    [SerializeField] private float flyingSaucersBossPlayerSpeed = 10.0f;
    private float flyingSaucersBossContainerOffset;


    // Boss 2
    [SerializeField] private GameObject motherShipperBossContainer;
    [SerializeField] private float motherShipperBossPlayerSpeed = 10.0f;
    private float motherShipperBossContainerOffset;

    // Boss 3
    [SerializeField] private GameObject finalBossContainer;
    [SerializeField] private float finalBossPlayerSpeed = 10.0f;
    [SerializeField] private FinalBossController finalBoss;
    private float finalBossContainerOffset;

    [SerializeField] private bool forceBoss = false;
    [SerializeField] private int forceLevel = 1;


    [SerializeField] private GameObject bossHealthBarContainer;
    [SerializeField] private GameObject gateCounterContainer;

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

        finalBossContainer.SetActive(false);
        finalBossContainerOffset = (
                finalBossContainer.transform.position -
                GameplayManager.Instance.Player.transform.position
            ).x;

        gateCounterContainer.SetActive(true);
        bossHealthBarContainer.SetActive(false);

        if (forceBoss)
        {
            GameManager.Instance.CurrentLevel = forceLevel - 1;
            ActivateBoss();
        }
    }

    public bool IsBossActive()
    {
        return bossActive;
    }

    public bool IsFinalBossActive()
    {
        return bossActive && GameManager.Instance.CurrentLevel == 2; // Zero-indexed, so level 3 actually,
                                                                     // there is the final boss
    }

    public FinalBossController GetFinalBoss()
    {
        return finalBoss;
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
            case 3:
                {
                    GameplayManager.Instance.Player.speed = finalBossPlayerSpeed;

                    float newX = GameplayManager.Instance.Player.transform.position.x
                        + finalBossContainerOffset;
                    Vector3 newPos = finalBossContainer.transform.position;
                    newPos.x = newX;

                    finalBossContainer.transform.position = newPos;
                    finalBossContainer.SetActive(true);
                    break;
                }
            default:
                break;
        }

        bossNameTag.SetActive(true);
        bossNameTagLabel.SetText(Loc.Get("gameplay_boss_intro_title_" + level.ToString()));
        bossNameLabel.SetText(Loc.Get("gameplay_boss_name_" + level.ToString()));
        StartCoroutine(DestroyBossNameTag());
        SoundManager.Instance.PlayBossMusic();

        gateCounterContainer.SetActive(false);
        bossHealthBarContainer.SetActive(true);
    }

    private IEnumerator DestroyBossNameTag()
    {
        yield return new WaitForSeconds(2f);
        Destroy(bossNameTag);
    }
}
