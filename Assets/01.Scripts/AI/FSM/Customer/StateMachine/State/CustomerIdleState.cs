public sealed class CustomerIdleState : BaseState<Customer, CustomerStateMachine>
{
    public CustomerIdleState(CustomerStateMachine p_StateMachine) : base(p_StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (m_Owner.Anim != null)
        {
            m_Owner.Anim.SetBool("Move", false);
            m_Owner.Anim.SetBool("Sit", false);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
