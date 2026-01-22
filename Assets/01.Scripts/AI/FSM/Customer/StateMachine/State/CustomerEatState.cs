public sealed class CustomerEatState : BaseState<Customer, CustomerStateMachine>
{
    public CustomerEatState(CustomerStateMachine p_StateMachine) : base(p_StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (m_Owner.Anim != null)
        {
            m_Owner.Anim.SetBool("Move", false);
            m_Owner.Anim.SetBool("Sit", true);
        }

        if (m_Owner.wantEat != null) m_Owner.wantEat.SetActive(false);
        if (m_Owner.satisfaction != null) m_Owner.satisfaction.SetActive(true);
    }

    public override void Exit()
    {
        if (m_Owner.Anim != null)
            m_Owner.Anim.SetBool("Sit", false);

        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
