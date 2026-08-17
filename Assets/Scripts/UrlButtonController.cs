using UnityEngine;

public class UrlButtonController : MonoBehaviour
{
    public void GoToUrl(string url)
    {
        Application.OpenURL(url);
    }
}
