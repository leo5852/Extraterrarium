using System.Xml;
using UnityEngine;

public class IdleState : IState
{
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Idle State");
    }
    public void Execute(EntityController entity)
    {
        if (entity.energy < 20f)
        {
            entity.ChangeState(new SleepState());
        }
        else if (entity.hunger < 50f)
        {
            entity.ChangeState(new HungerState());
        }
    }
    public void Exit(EntityController entity)
    {
        Debug.Log("Exit: Idle State");
    }
}

public class HungerState : IState
{
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Hunger State");
    }
    public void Execute(EntityController entity)
    {
        entity.LookForFood();
    }
    public void Exit(EntityController entity)
    {
        Debug.Log("Exit: Hunger State");
    }
}
public class SleepState : IState
{
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Sleep State");
    }

    public void Execute(EntityController entity)
    {
        // add sleep animation
        
        entity.RestoreEnergy(10f * Time.deltaTime);

        // 에너지가 꽉 차면 기상 후 idle state복귀
        if (entity.energy >= 100f)
        {
            entity.ChangeState(new IdleState());
        }
    }

    public void Exit(EntityController entity)
    {
        Debug.Log("Exit: Sleep State");
    }
}