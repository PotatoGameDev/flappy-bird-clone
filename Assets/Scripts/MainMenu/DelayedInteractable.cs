using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class DelayedInteractable : MonoBehaviour
{
    [SerializeField]
    private float waitSeconds = 2;
    private WaitForSecondsRealtime waitTime;

    private Button button;


    void Awake()
    {
        button = GetComponent<Button>();

        waitTime = new WaitForSecondsRealtime(waitSeconds);
    }

    void OnEnable()
    {
        button.interactable = false;
        StartCoroutine(SetInteractable());
    }

    private IEnumerator SetInteractable()
    {
        yield return waitTime;

        button.interactable = true;
    }

    void OnDisable()
    {
        button.interactable = false;
    }
}
