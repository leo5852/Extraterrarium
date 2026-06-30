using UnityEngine;

public class IdleState : IState
{
    public void Enter(EntityController entity)
    {
        Debug.Log("Entering Idle State!");
    }
    public void Execute(EntityController entity)
    {
        Debug.Log("Executing Idle State...");
    }
    public void Exit(EntityController entity)
    {
        Debug.Log("Exiting Idle State!");
    }
}

public class HungerState : IState
{
    public void Enter(EntityController entity)
    {
        Debug.Log("Entering Hunger State!");
    }
    public void Execute(EntityController entity)
    {
        Debug.Log("Executing Hunger State...");
    }
    public void Exit(EntityController entity)
    {
        Debug.Log("Exiting Hunger State!");
    }
}
