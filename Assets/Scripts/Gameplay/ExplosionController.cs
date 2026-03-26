using UnityEngine;
using UnityEngine.Events;
using PotatoGameDev.Pool;

public class ExplosionController : PooledInstance
{
    public UnityEvent OnFinished;

    public new void Init()
    {
        transform.localScale = Vector2.one;
    }

    public void OnAnimationFinished()
    {
        OnFinished?.Invoke();
        Release();
    }

    public new void Release()
    {
        OnFinished.RemoveAllListeners();
        base.Release();
    }
}
