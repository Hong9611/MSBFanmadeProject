public abstract class StateMachine<TOwner>
{
    protected IState currentState;

    public TOwner Owner { get; }

    protected StateMachine(TOwner p_Owner)
    {
        Owner = p_Owner;
    }

    public virtual void ChangeState(IState state)
    {
        currentState?.Exit();
        currentState = state;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
