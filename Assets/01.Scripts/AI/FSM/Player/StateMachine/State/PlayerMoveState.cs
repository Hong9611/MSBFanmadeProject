using UnityEngine;

public class PlayerMoveState : BaseState<Player, PlayerStateMachine>
{
    public PlayerMoveState(PlayerStateMachine p_StateMachine) : base(p_StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("MOVE");
    }

    public override void Update()
    {
        if (m_Owner.input.moveInput.sqrMagnitude < 0.01f)
        {
            m_StateMachine.ChangeState(m_StateMachine.IdleState);
        }

        m_Owner.input.MoveUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
