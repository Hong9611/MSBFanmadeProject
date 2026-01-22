public abstract class BaseState<TOwner, TStateMachine> : IState
    where TStateMachine : StateMachine<TOwner>
{
    protected readonly TStateMachine m_StateMachine;
    protected TOwner m_Owner;

    protected BaseState(TStateMachine p_StateMachine)
    {
        m_StateMachine = p_StateMachine;
    }

    public virtual void Enter()
    {
        m_Owner = m_StateMachine.Owner;
    }

    public virtual void Exit()
    {

    }

    public virtual void Update()
    {

    }
}