using UnityEngine;
using UnityEngine.UI;

public class SystemMenuController : SecondaryMenuDelegate
{
    [SerializeField] private Button startButton;

    void Start()
    {
        UpdateMenu();
    }

    public override void UpdateMenu()
    {
        startButton.gameObject.SetActive(false);
    }
}
