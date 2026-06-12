using System.Collections.Generic;
using UnityEngine;

public class SupportRat : BasicRat
{
    private readonly List<BasicRat> ratsToHeal = new List<BasicRat>();

    public float healingAmount;
    public float healingProbability = 0.85f;

    public override void PerformAction()
    {
        HealNearestRat();
    }

    // Animation Event opcional. Solo cura si "Action From Animation Event" está activado en el inspector.
    public void Heal()
    {
        PerformActionFromAnimationEvent();
    }

    private void HealNearestRat()
    {
        ratsToHeal.Clear();
        OrderRatsByLowestHealth();

        if (ratsToHeal.Count == 0)
        {
            return;
        }

        healingAmount = Random.Range(20f, 40f);

        BasicRat target = RollDice(healingProbability)
            ? ratsToHeal[0]
            : ratsToHeal[Random.Range(0, ratsToHeal.Count)];

        target.health = Mathf.Clamp(
            target.health + healingAmount,
            0f,
            target.data.InitialHealth
        );

        if (DebugCombat)
        {
            Debug.Log($"{name}: cura aplicada a {target.name}: +{healingAmount}.", this);
        }
    }

    private void OrderRatsByLowestHealth()
    {
        if (waveManager == null || waveManager.ratsPerWave == null)
        {
            return;
        }

        foreach (BasicRat rat in waveManager.ratsPerWave)
        {
            if (rat == null || rat == this || !rat.gameObject.activeInHierarchy || rat.health <= 0f)
            {
                continue;
            }

            if (Vector3.Distance(transform.position, rat.transform.position) <= data.ActionRange)
            {
                ratsToHeal.Add(rat);
            }
        }

        ratsToHeal.Sort((a, b) => a.health.CompareTo(b.health));
    }
}
