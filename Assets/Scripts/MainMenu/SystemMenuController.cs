using UnityEngine;

public class SystemMenuController : SecondaryMenuDelegate
{
    [SerializeField] private FakeButton startButton;

    void Start()
    {
        UpdateMenu();
    }

    public override void UpdateMenu()
    {
        startButton.gameObject.SetActive(false);
    }
}
