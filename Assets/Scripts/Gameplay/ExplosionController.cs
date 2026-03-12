using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    public void OnAnimationFinished()
    {
        GameplayManager.Instance.Player.DeathAnimationEnded();
    }
}
