using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private string sceneToLoad = "Gameplay";

    void Start()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        InstancePoolsManager.Instance.EnergyBallControllerPool.Preheat(50);

        if (GameManager.Instance.levelSettings.levelType == LevelType.BossFight)
        {
            if (GameManager.Instance.CurrentLevel < 2)
            {
                InstancePoolsManager.Instance.BulletControllerPool.Preheat(50);
            }
            else
            {
                InstancePoolsManager.Instance.RocketControllerPool.Preheat(5);
                InstancePoolsManager.Instance.TinyRocketControllerPool.Preheat(10);
            }
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

