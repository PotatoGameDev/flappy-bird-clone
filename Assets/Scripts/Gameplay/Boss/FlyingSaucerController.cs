using UnityEngine;
using DG.Tweening;

public class FlyingSaucerController : MonoBehaviour
{
    [SerializeField] private float riseY = 3f;
    [SerializeField] private float dropY = -3f;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float shootPauseDuration = 1f;

    void Start()
    {
        DOVirtual.DelayedCall(Random.Range(0f, 2f), StartCycle);
    }

    void StartCycle()
    {
        Sequence seq = DOTween.Sequence();

        // Rise up
        seq.Append(transform.DOMoveY(riseY, moveDuration).SetEase(Ease.InOutSine));
        // Shoot at the top
        seq.AppendCallback(ShootAtPlayer);
        seq.AppendInterval(shootPauseDuration);

        // Drop down
        seq.Append(transform.DOMoveY(dropY, moveDuration).SetEase(Ease.InOutSine));
        // Shoot at the bottom
        seq.AppendCallback(ShootAtPlayer);
        seq.AppendInterval(shootPauseDuration);
        seq.Join(transform.DOMoveX(Random.Range(-3f, 3f), moveDuration).SetEase(Ease.InOutSine));

        // Loop forever
        seq.SetLoops(-1);
    }

    void ShootAtPlayer()
    {
        // hook up your existing projectile logic here
        Debug.Log("Pew pew!");
    }

}
