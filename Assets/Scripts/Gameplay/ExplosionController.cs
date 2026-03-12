using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public void OnAnimationFinished()
    {
        GameManager.Instance.Player.DeathAnimationEnded();
    }
}
