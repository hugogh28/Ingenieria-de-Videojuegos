using UnityEngine;
using System.Collections.Generic;

public static class RatDataFactory
{
    public const float GlobalDetectionRange = 10000f;

    private const float CommonNormalActionRange = 2.2f;
    private const float CommonTankActionRange = 2.8f;

    private const int CommonPoints = 100;
    private const int TankPoints = 200;
    private const int SpecialPoints = 150;
    private const int SpecialTankPoints = 300;

    private static readonly Dictionary<(RatType, RatSubType), RatData> cache = new();

    public static RatData GetRatData(RatType type,  RatSubType subtype)
    {
        var key = (type, subtype);

        if(cache.ContainsKey(key)) return cache[key];

        RatData data = CreateRatData(type, subtype);
        cache.Add(key, data);

        return data;
    }

    private static RatData CreateRatData(RatType type, RatSubType subtype)
    {
        switch(type, subtype)
        {
            case (RatType.CommonRat, RatSubType.Normal):
                return new RatData(
                    RatType.CommonRat,
                    RatSubType.Normal,
                    initialHealth: 100f,
                    attackDamage: 5f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: CommonPoints,
                    actionRange: CommonNormalActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 1f,
                    actionNextToPlayer: "attack"
                    );
            case (RatType.CommonRat, RatSubType.Tank):
                return new RatData(
                    RatType.CommonRat,
                    RatSubType.Tank,
                    initialHealth: 200f,
                    attackDamage: 10f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: TankPoints,
                    actionRange: CommonTankActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 1.2f,
                    actionNextToPlayer: "attack"
                    );
            case (RatType.ShooterRat, RatSubType.Normal):
                return new RatData(
                    RatType.ShooterRat,
                    RatSubType.Normal,
                    initialHealth: 100f,
                    attackDamage: 15f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: SpecialPoints,
                    actionRange: CommonNormalActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 2f,
                    actionNextToPlayer: "shoot"
                    );
            case (RatType.ShooterRat, RatSubType.Tank):
                return new RatData(
                    RatType.ShooterRat,
                    RatSubType.Tank,
                    initialHealth: 200f,
                    attackDamage: 20f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: SpecialTankPoints,
                    actionRange: CommonTankActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 2.5f,
                    actionNextToPlayer: "shoot"
                    );

            case(RatType.SupportRat, RatSubType.Normal):
                return new RatData(
                    RatType.SupportRat,
                    RatSubType.Normal,
                    initialHealth: 100f,
                    attackDamage: 20f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: SpecialPoints,
                    actionRange: CommonNormalActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 4f,
                    actionNextToPlayer: "heal"
                    );

            case (RatType.SupportRat, RatSubType.Tank):
                return new RatData(
                    RatType.SupportRat,
                    RatSubType.Tank,
                    initialHealth: 200f,
                    attackDamage: 30f,
                    criticProbability: 0.25f,
                    pointsGivenAtDeath: SpecialTankPoints,
                    actionRange: CommonTankActionRange,
                    detectionRange: GlobalDetectionRange,
                    delay: 6f,
                    actionNextToPlayer: "heal"
                    );

            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
}
