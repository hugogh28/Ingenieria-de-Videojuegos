using UnityEngine;

public sealed class RatAttackState : IRatState
{
    private float cooldownTimer;

    public void Enter(BasicRat rat)
    {
        cooldownTimer = 0f;
        rat.SetAgentStopped(true);
        rat.SetWalkingAnimation(false);
        TriggerAction(rat);
    }

    public void Tick(BasicRat rat)
    {
        rat.UpdateSensing();

        if (!rat.IsPlayerDetected)
        {
            rat.ChangeState(rat.IdleState);
            return;
        }

        if (!rat.IsPlayerInActionRange)
        {
            rat.ChangeState(rat.ChaseState);
            return;
        }

        rat.SetAgentStopped(true);
        rat.SetWalkingAnimation(false);
        rat.FacePlayer();

        cooldownTimer -= Time.deltaTime;
        rat.doneSomething = cooldownTimer > 0f;

        if (cooldownTimer <= 0f)
        {
            TriggerAction(rat);
        }
    }

    public void Exit(BasicRat rat)
    {
        rat.doneSomething = false;
    }

    private void TriggerAction(BasicRat rat)
    {
        rat.TriggerActionAnimation();
        rat.PerformActionFromState();

        cooldownTimer = Mathf.Max(0.05f, rat.data.Delay);
        rat.doneSomething = true;
    }
}
