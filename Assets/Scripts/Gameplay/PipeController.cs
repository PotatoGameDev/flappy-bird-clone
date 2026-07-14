using UnityEngine;

public class PipeController : MonoBehaviour, IPlayerHitReceiver
{
    public bool IsHittable() => true;
    public long GetLifeUnit() => 1_000_000L;
    public void PlayerHit(Damage damage) { }
    public bool CanBeDamaged() => false;
}
