using UnityEngine;

public class DamageCalculator : MonoBehaviour
{
    private readonly float RPM_PENALTY_THRESHOLD = 20f;
    // This is when the damage is maximal:
    private readonly float RPM_MAX = 80f;

    // how many people die each second if we reach RPM_MAX:
    private readonly long PEOPLE_DIED_MAX_RPM = 1_000_000;

    // how many people die per second of being at the min dist to the edge
    // up or down.
    private readonly long PEOPLE_DIED_MAX_OUT_OF_BOUNDS = 100_000;

    // TODO move to PlanetController?
    [SerializeField] private EnergyShieldController energyShieldController;

    // This is where we deal the max damage, over this value we just clamp to max.
    [SerializeField] private float maxHandledRelativeSpeed = 20f;

    // Base damage factor per distance.
    [SerializeField]
    private AnimationCurve damageCurve = new(
        new Keyframe(0f, 1f, 0f, 0f),
        new Keyframe(0.3f, 1f, 0f, -5f),
        new Keyframe(1f, 0f, 0f, 0f)
    );

    public Damage CalculateDamage(float dist, long baseLifeUnit, Vector2 relativeVelocity, bool shielded)
    {
        float baseDamageFactor;

        shielded = shielded && GameplayManager.Instance.ShieldAvailable();

        if (!shielded)
        {
            // If it's unshielded, we get the full force, no need for evaluation
            baseDamageFactor = 1f;
        }
        else
        {
            baseDamageFactor = EvaluateDamageFactor(dist);
        }

        if (baseDamageFactor == 0f) return Damage.zero;

        // Physics relative speed based, notice it cannot exceed 1.
        float relativeSpeed = relativeVelocity.magnitude;
        float hitForceFactor = Mathf.Clamp(
                relativeSpeed,
                0f,
                maxHandledRelativeSpeed
                )
            / maxHandledRelativeSpeed;

        // By convention, we say that if factor reaches 1 or more on the graph,
        // it's the parry zone.
        // If it's not shielded, it obviously is not parried as well.
        // Remember of it when changing the graph!
        bool parried = shielded && (baseDamageFactor >= 1f);

        // Simple formula!
        long totalEnemyDamage = (long)(baseDamageFactor * hitForceFactor * baseLifeUnit);

        // This is a design decision:
        // The player gets the same shielded damage as the enemy.
        // So the closer you get when shielding, the more damage you get, but also the enemy gets it.
        // But, if you are brave enough, you get close enough to the parry distance and your damage goes to zero.
        //
        // Other ways could be:
        // - the player could get all the damage of the enemy that was not shielded (max enemy dmg - shielded enemy dmg)
        // - the player could get plain 0 dmg if shielded. 
        //
        // This decision has to be taken into consideration when designing the enemies and their max life...
        //
        // EDIT: Actually, if the player gets the same damage,
        // then this beats the purpose of the shield :D
        // So the decision is, parry is just for visuals and some
        // additional effects like bumping rockets off.
        // Shield reduces all the damage it can.
        //
        long totalPlayerDamage = (shielded || dist > 0f)
            ? 0L
            : totalEnemyDamage; // Direct hit or no shield left.

        // This gives 0->1(or more) times 0->1 times baseLifeUnit.
        // So potentially it could result in more then life unit damage,
        // but this is limited by the graph - it's max value set in editor.
        // The physics will not blow the damage out of proportion.
        Damage result = new(
            totalEnemyDamage,
            totalPlayerDamage,
            parried,
            shielded,
            relativeVelocity
            );

        return result;
    }

    public Damage CalculateRotationalDamage(float rpm)
    {
        float rpmAbs = Mathf.Abs(rpm);

        if (rpmAbs > RPM_PENALTY_THRESHOLD)
        {
            float penaltyRpm = rpmAbs - RPM_PENALTY_THRESHOLD;
            float maxRpm = RPM_MAX - RPM_PENALTY_THRESHOLD;

            long totalPeopleDied = (long)(
                    (float)(penaltyRpm / maxRpm) * PEOPLE_DIED_MAX_RPM
                    );

            return new(0, totalPeopleDied, false, false, Vector2.zero);
        }

        return Damage.zero;
    }

    public Damage CalculateOutOfBoundsDamage(float outOfBoundsFraction)
    {
        outOfBoundsFraction = Mathf.Clamp01(outOfBoundsFraction);

        if (outOfBoundsFraction > 0f)
        {
            long totalPeopleDied = (long)(
                    outOfBoundsFraction * PEOPLE_DIED_MAX_OUT_OF_BOUNDS
                    );

            return new(0, totalPeopleDied, false, false, Vector2.zero);
        }

        return Damage.zero;
    }

    private float GetMaxDistance()
    {
        // The scale will change
        return energyShieldController.GetBaseRadius();
    }

    /**
     * Calculates base damage factor.
     * This is applied to both the player and the enemy.
     *
     * You have to provide actual distance between objects, and max distance that you want to handle.
     * It will start with max damage (it should) and go to 0 at maxDist.
     * The rest is interpolated or configured on the graph in the editor.
     * Like the "Parry" distance is decided by setting it up in the editor.
     * Parry is just when the function reaches the value of 1 or more.
     *
     */
    private float EvaluateDamageFactor(float dist)
    {
        if (dist < 0f)
        {
            dist = 0f;
        }

        float maxDist = GetMaxDistance();

        if (dist > maxDist)
        {
            // No damage
            return 0f;
        }

        float interpolatedDist = dist / maxDist;

        float rawValue = damageCurve.Evaluate(interpolatedDist);

        Debug.Log("Dist: " + dist + " ~ " + interpolatedDist + " maxDist: " + maxDist + " result = " + rawValue + " parry: " + (rawValue >= 1.0f));

        return Mathf.Max(0f, rawValue); // Damage factor can be greater than 1
    }
}

public struct Damage
{
    public static readonly Damage zero = new(0L, 0L, false, false, Vector2.zero);

    public Damage(
            long enemy,
            long player,
            bool parried,
            bool shielded,
            Vector2 relVel
            )
    {
        this.enemy = enemy;
        this.player = player;
        this.parried = parried;
        this.shielded = shielded;
        this.relVel = relVel;
    }

    public long enemy;
    public long player;
    public bool parried;
    public bool shielded;
    public Vector2 relVel;

    public readonly override string ToString()
    {
        return $"e:{enemy},p:{player},par:{parried},shielded:{shielded}";
    }

}
