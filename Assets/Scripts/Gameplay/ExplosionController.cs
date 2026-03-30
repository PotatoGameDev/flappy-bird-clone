using UnityEngine;
using UnityEngine.Events;
using PotatoGameDev.Pool;

[RequireComponent(typeof(Animator))]
public class ExplosionController : PooledInstance
{
    private UnityEvent _onFinishedEvent = new();

    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public UnityEvent OnFinished
    {
        get
        {
            return _onFinishedEvent;
        }

        set
        {
            _onFinishedEvent = value;
        }
    }

    private void OnEnable()
    {
        anim.enabled = true;
        anim.Rebind();
        anim.Update(0f);
    }

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
        OnFinished?.RemoveAllListeners();
        base.Release();
    }
}
