public interface IPlayerHitReceiver
{
    void PlayerHit(Damage damage);

    long GetLifeUnit();

    bool IsHittable();

    bool CanBeDamaged() => true;
}

