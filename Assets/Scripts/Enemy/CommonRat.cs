using UnityEngine;

public class CommonRat : BasicRat
{
    public override void PerformAction()
    {
        float damage = data.AttackDamage;

        if (RollDice(data.CriticProbability))
        {
            float criticImpact = Random.Range(1f, 2f);
            damage *= criticImpact;
        }

        bool damageApplied = TryDamageObservedPlayer(damage, requireActionRange: true);

        if (DebugCombat && damageApplied)
        {
            Debug.Log($"{name}: daño cuerpo a cuerpo aplicado al PlayerController: {damage}. El observer de vida queda notificado por PlayerController.HealthChanged.", this);
        }
    }

    // Animation Event opcional. Solo hace daño si "Action From Animation Event" está activado en el inspector.
    public void Melee()
    {
        PerformActionFromAnimationEvent();
    }
}
