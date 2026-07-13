public interface IPlayerHitReceiver
{

    private const float MAGNITUDE_DAMAGE_FACTOR = 20f;

    public static float CalculateHitFraction(PlayerHitType type)
    {
        float parriedFactor = type switch
        {
            PlayerHitType.PARRY => 1f, // Promote risky gameplay...
            PlayerHitType.HIT => 0.1f,
            PlayerHitType.SHIELD => 0.5f, // ...against playing safe
            _ => 0f,
        };
        return parriedFactor;
    }

    public void PlayerHit(PlayerHitType type);

}

public enum PlayerHitType
{
    HIT, PARRY, SHIELD
}
