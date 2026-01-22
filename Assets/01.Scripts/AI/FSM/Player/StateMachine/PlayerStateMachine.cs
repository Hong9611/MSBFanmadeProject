public sealed class PlayerStateMachine : StateMachine<Player>
{
    public PlayerIdleState IdleState { get; }
    public PlayerMoveState MoveState { get; }

    public PlayerStateMachine(Player p_Unit) : base(p_Unit)
    {
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
    }
}
