using UnityEngine;

public class PlayerIdleState : BaseState<Player, PlayerStateMachine>
{
    public PlayerIdleState(PlayerStateMachine p_StateMachine) : base(p_StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        m_Owner.input.IdleUpdate();
        Debug.Log("IDLE");
    }

    public override void Update()
    {
        if (m_Owner.input.moveInput.sqrMagnitude > 0.01f)
        {
            m_StateMachine.ChangeState(m_StateMachine.MoveState);
        }

        m_Owner.input.IdleUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
