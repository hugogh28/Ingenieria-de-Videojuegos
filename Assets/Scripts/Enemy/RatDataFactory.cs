using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public static class RatDataFactory
{
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
                    pointsGivenAtDeath: 5,
                    actionRange: 3f, 
                    detectionRange: 1000f,
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
                    pointsGivenAtDeath: 10,
                    actionRange: 4f,
                    detectionRange: 1000f,
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
                    pointsGivenAtDeath: 20,
                    actionRange: 14f,
                    detectionRange: 20f,
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
                    pointsGivenAtDeath: 40,
                    actionRange: 14f,
                    detectionRange: 22f,
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
                    pointsGivenAtDeath: 35,
                    actionRange: 12f,
                    detectionRange: 10f,
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
                    pointsGivenAtDeath: 75,
                    actionRange: 14f,
                    detectionRange: 15f,
                    delay: 6f,
                    actionNextToPlayer: "heal"
                    );

            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
}
