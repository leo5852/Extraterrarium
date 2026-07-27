using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;


public class IdleState : IState
{
    // TODO: refactor 필요
    public const float TRIGGER_HUNGER = 60f;
    public const float TRIGGER_ENERGY = 10f;
    
    private bool isWandering = false;
    private float wanderRadius; // 배회 반경
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Idle State");
        wanderRadius = entity.wanderRadius;
        isWandering = false;
        entity.PlayAnimation("Idle");
    }
    // 프레임 단위: 도착했는지만 가볍게 확인
    public void ExecuteFrame(EntityController entity)
    {
        if (isWandering)
        {
            if (entity.IsArrivedAtTarget())
            {
                isWandering = false;
                entity.PlayAnimation("Idle"); // 다시 대기 애니메이션
            }
        }
    }

    // 틱 단위: 무거운 상태전환 검사/배회 판단
    public void ExecuteTick(EntityController entity)
    {
        // 상태 전환 검사
        if (entity.energy < TRIGGER_ENERGY)
        {
            entity.ChangeState(new SleepState());
            return;
        }
        else if (entity.hunger < TRIGGER_HUNGER)
        {
            entity.ChangeState(new HungerState());
            return;
        }

        // 멈춰있으면 배회 로직 시작
        if (!isWandering)
        {
            // 매 틱마다 50% 확률로 새로운 곳으로 wander
            if (Random.value > 0.5f)
            {
                Vector3 randomDest = entity.GetRandomNavMeshPoint(wanderRadius);
                entity.MoveTo(randomDest);

                isWandering = true;
                entity.PlayAnimation("Walk"); // 걷기 애니메이션 재생
            }

        }
        else 
        { 
        
        }
    }
    public void Exit(EntityController entity)
    {
        isWandering = false;
        Debug.Log("Exit: Idle State");
    }
}

public class HungerState : IState
{
    private IConsumable targetFood;
    private float eatSpeed;
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Hunger State");
        eatSpeed = entity.eatSpeed;
        entity.isEating = false;
        targetFood = entity.LookForFood();

        if (targetFood != null)
        {
            Debug.Log("Food found!");
            Vector3 foodPosition = ((Component)targetFood).transform.position;
            entity.MoveTo(foodPosition);
        }
        else
        {
            // return to idle if no food found
            Debug.Log("Can't find food...");
            entity.ChangeState(new IdleState());
        }
    }
    public void ExecuteFrame(EntityController entity)
    {
        if (targetFood == null || ((Component)targetFood) == null)
        {
            Debug.Log("Lost target...Searching again.");
            entity.ChangeState(new HungerState()); // Hunger State 재시작
            return;
        }

        if (!entity.isEating)
        {
            //Still Chasing food
            if (entity.IsArrivedAtTarget())
            {
                entity.isEating = true;
                Debug.Log("Caputured Food! Eating...");
                // 먹는 애니메이션 재생
            }
        }
        else
        {
            float requestAmount = eatSpeed * Time.deltaTime;
            float actuallyEaten = targetFood.ConsumeNutrition(requestAmount);
            entity.RestoreHunger(actuallyEaten);

            if (actuallyEaten < requestAmount)
            {
                entity.ChangeState(new HungerState());
            }
        }
    }
    public void ExecuteTick(EntityController entity)
    {
        // 틱이 돌았는데 쫓던 먹이가 사라졌다면?
        if (targetFood == null || ((Component)targetFood) == null)
        {
            entity.ChangeState(new HungerState());
            return;
        }

        // 틱이 돌았을 때 배가 다 찼다면? (틱 단위로 체크해도 충분함)
        if (entity.hunger >= 100f)
        {
            entity.ChangeState(new IdleState());
        }
    }
    public void Exit(EntityController entity)
    {
        entity.isEating = false;
        Debug.Log("Exit: Hunger State");
    }
}
public class SleepState : IState
{
    private float sleepRestoreSpeed = 15f;
    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Sleep State");
        entity.isSleeping = true;
        sleepRestoreSpeed = entity.sleepRestoreSpeed;

        // 걷던 중이면 멈추기
        entity.agent.ResetPath(); 

        // 수면 애니메이션 재생
        // entity.PlayAnimation("Sleep");
    }

    // 프레임 단위: 자는 동안 에너지를 실시간으로 부드럽게 회복
    public void ExecuteFrame(EntityController entity)
    {
        entity.RestoreEnergy(sleepRestoreSpeed * Time.deltaTime);
    }

    // 틱 단위 실행: 에너지가 회복 검사하고 기상 판단
    public void ExecuteTick(EntityController entity)
    {
        // 충분히 쉬었으면 기상!
        if (entity.energy >= 100f)
        {
            entity.ChangeState(new IdleState());
        }
    }

    public void Exit(EntityController entity)
    {
        Debug.Log("Exit: Sleep State");

        entity.isSleeping = false;
        // entity.StopAnimation("Sleep");
    }
}