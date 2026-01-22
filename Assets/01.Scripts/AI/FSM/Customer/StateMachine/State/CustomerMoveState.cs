public sealed class CustomerMoveState : BaseState<Customer, CustomerStateMachine>
{
    public CustomerMoveState(CustomerStateMachine p_StateMachine) : base(p_StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        m_Owner.TryStartMove(m_StateMachine.RequestedDestination, m_StateMachine.RequestedLookAt);
    }

    public override void Exit()
    {
        m_Owner.StopMovementCoroutine();
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
