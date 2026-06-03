using UnityEngine;

public enum RatType
{
    CommonRat,
    ShooterRat,
    SupportRat
}

public enum RatSubType
{
    Normal,
    Tank
}

public class RatData
{
    public RatType Type { get; private set; }
    public RatSubType SubType { get; private set; }

    public float InitialHealth { get; private set; }
    public float AttackDamage { get; private set; }
    public float CriticProbability {  get; private set; }
    public int PointsGivenAtDeath { get; private set; }


    public float ActionRange { get; private set; }
    public float DetectionRange { get; private set; }
    public float Delay {  get; private set; }

    public string ActionNextToPlayer {  get; private set; }

    public RatData(
        RatType type,
        RatSubType subType,
        float initialHealth,
        float attackDamage,
        float criticProbability,
        int pointsGivenAtDeath,
        float actionRange,
        float detectionRange,
        float delay,
        string actionNextToPlayer
        )
    {
        Type = type;
        SubType = subType;
        InitialHealth = initialHealth;
        AttackDamage = attackDamage;
        CriticProbability = criticProbability;
        PointsGivenAtDeath = pointsGivenAtDeath;
        ActionRange = actionRange;
        DetectionRange = detectionRange;
        Delay = delay;
        ActionNextToPlayer = actionNextToPlayer;
    }
}
