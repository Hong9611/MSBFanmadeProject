using UnityEngine;

public sealed class CustomerStateMachine : StateMachine<Customer>
{
    public CustomerIdleState IdleState { get; }
    public CustomerMoveState MoveState { get; }
    public CustomerEatState EatState { get; }

    public Vector3 RequestedDestination { get; private set; }
    public Vector3? RequestedLookAt { get; private set; }

    public CustomerStateMachine(Customer p_Owner) : base(p_Owner)
    {
        IdleState = new CustomerIdleState(this);
        MoveState = new CustomerMoveState(this);
        EatState = new CustomerEatState(this);
    }

    public void RequestMove(Vector3 p_Destination, Vector3? p_LookAt = null)
    {
        RequestedDestination = p_Destination;
        RequestedLookAt = p_LookAt;
        ChangeState(MoveState);
    }
}
