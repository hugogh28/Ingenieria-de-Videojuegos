public sealed class RatIdleState : IRatState
{
    public void Enter(BasicRat rat)
    {
        rat.SetAgentStopped(true);
        rat.SetWalkingAnimation(false);
        rat.doneSomething = false;
    }

    public void Tick(BasicRat rat)
    {
        rat.UpdateSensing();

        if (!rat.IsPlayerDetected)
        {
            return;
        }

        if (rat.IsPlayerInActionRange)
        {
            rat.ChangeState(rat.AttackState);
            return;
        }

        rat.ChangeState(rat.ChaseState);
    }

    public void Exit(BasicRat rat)
    {
    }
}
