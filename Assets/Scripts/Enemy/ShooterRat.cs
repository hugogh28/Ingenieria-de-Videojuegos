using System.Linq;
using UnityEngine;

public class ShooterRat : BasicRat
{
    public Transform shotPos;
    public float deviationRadius = 1.5f;
    public float maxDistanceHit = 50f;

    [Header("Shooter")]
    [SerializeField] private bool requireLineOfSight = false;

    public override void PerformAction()
    {
        PlayerController playerController = GetPlayerController();

        if (playerController == null)
        {
            Debug.LogWarning("ShooterRat no encuentra PlayerController.", this);
            return;
        }

        UpdateSensing();

        if (!IsPlayerInActionRange)
        {
            if (DebugCombat)
            {
                Debug.Log($"{name}: disparo cancelado, jugador fuera de rango.", this);
            }

            return;
        }

        if (requireLineOfSight && !HasLineOfSightToPlayer(playerController))
        {
            if (DebugCombat)
            {
                Debug.Log($"{name}: disparo cancelado, sin línea de visión.", this);
            }

            return;
        }

        float damage = data.AttackDamage;

        if (RollDice(data.CriticProbability))
        {
            float criticImpact = Random.Range(1f, 1.5f);
            damage *= criticImpact;
        }

        bool damageApplied = TryDamageObservedPlayer(damage, requireActionRange: true);

        if (DebugCombat && damageApplied)
        {
            Debug.Log($"{name}: daño de disparo aplicado al PlayerController: {damage}. El observer de vida queda notificado por PlayerController.HealthChanged.", this);
        }
    }

    // Animation Event opcional. Solo hace daño si "Action From Animation Event" está activado en el inspector.
    public void Shoot()
    {
        PerformActionFromAnimationEvent();
    }

    private bool HasLineOfSightToPlayer(PlayerController playerController)
    {
        Transform originTransform = shotPos != null ? shotPos : transform;
        Vector3 origin = shotPos != null ? shotPos.position : transform.position + Vector3.up;

        Vector2 deviationCircle = Random.insideUnitCircle * deviationRadius;
        Vector3 target = playerController.transform.position + originTransform.right * deviationCircle.x + originTransform.up * deviationCircle.y;
        Vector3 direction = (target - origin).normalized;

        Debug.DrawRay(origin, direction * maxDistanceHit, Color.red, 1f);

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistanceHit, ~0, QueryTriggerInteraction.Ignore)
            .OrderBy(hit => hit.distance)
            .ToArray();

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            PlayerController hitPlayer = hit.collider.GetComponentInParent<PlayerController>();
            return hitPlayer == playerController;
        }

        return false;
    }
}
