public sealed class RatStateMachine
{
    public IRatState CurrentState { get; private set; }

    public void ChangeState(BasicRat rat, IRatState nextState)
    {
        if (rat == null || nextState == null || CurrentState == nextState)
        {
            return;
        }

        CurrentState?.Exit(rat);
        CurrentState = nextState;
        CurrentState.Enter(rat);
    }

    public void Tick(BasicRat rat)
    {
        CurrentState?.Tick(rat);
    }

    public void Clear(BasicRat rat)
    {
        CurrentState?.Exit(rat);
        CurrentState = null;
    }
}
