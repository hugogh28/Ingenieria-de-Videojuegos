public sealed class RatChaseState : IRatState
{
    public void Enter(BasicRat rat)
    {
        rat.SetAgentStopped(false);
        rat.SetWalkingAnimation(true);
        rat.doneSomething = false;
    }

    public void Tick(BasicRat rat)
    {
        rat.UpdateSensing();

        if (!rat.IsPlayerDetected)
        {
            rat.ChangeState(rat.IdleState);
            return;
        }

        if (rat.IsPlayerInActionRange)
        {
            rat.ChangeState(rat.AttackState);
            return;
        }

        rat.FacePlayer();
        rat.MoveToPlayer();
        rat.SetWalkingAnimation(true);
    }

    public void Exit(BasicRat rat)
    {
        rat.SetWalkingAnimation(false);
    }
}
