using UnityEngine;

public interface IEventObject
{
    public void OnUnitEnter(Collider Unit);

    public void OnUnitExit(Collider Unit);
}
