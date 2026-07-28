using UnityEngine;

public class IdleState : IState
{
    // TODO: refactor 필요
    private bool isWandering = false;
    private float wanderRadius;
    private float hungerTrigger;
    private float energyTrigger;

    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Idle State");
        wanderRadius = entity.wanderRadius;
        hungerTrigger = entity.species.hungerTrigger;
        energyTrigger = entity.species.energyTrigger;
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
        if (entity.energy < energyTrigger)
        {
            entity.ChangeState(new SleepState());
            return;
        }
        else if (entity.hunger < hungerTrigger)
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
    private bool isWandering = false;

    public void Enter(EntityController entity)
    {
        Debug.Log("Enter: Hunger State");
        eatSpeed = entity.eatSpeed;

        // 진입할 때 이전 상태를 초기화
        entity.isEating = false;
        isWandering = false;
        targetFood = null;

        SearchOrWander(entity);
    }

    private void SearchOrWander(EntityController entity)
    {
        // 먹이를 찾기 전에 먹는 행위를 무조건 강제 종료!
        entity.isEating = false;

        targetFood = entity.LookForFood();

        // 찾은 먹이가 존재하고, 파괴되지 않은(안전한) 상태인지 이중 체크
        if (targetFood != null && ((Component)targetFood) != null)
        {
            isWandering = false;
            Vector3 foodPosition = ((Component)targetFood).transform.position;
            entity.MoveTo(foodPosition);
            entity.PlayAnimation("Walk");
        }
        else
        {
            isWandering = true;
            Vector3 randomDest = entity.GetRandomNavMeshPoint(entity.wanderRadius);
            entity.MoveTo(randomDest);
            entity.PlayAnimation("Walk");
        }
    }

    public void ExecuteFrame(EntityController entity)
    {
        // 1. 타겟 먹이가 존재하고, 씬에서 파괴되지 않았는지 체크
        if (targetFood != null && ((Component)targetFood) != null)
        {
            if (!entity.isEating)
            {
                // 먹이를 향해 걸어가는 중
                if (entity.IsArrivedAtTarget())
                {
                    entity.isEating = true;
                    entity.PlayAnimation("Idle");
                }
            }
            else
            {
                // 밥을 먹는 중
                float requestAmount = eatSpeed * Time.deltaTime;
                float actuallyEaten = targetFood.ConsumeNutrition(requestAmount);
                entity.RestoreHunger(actuallyEaten);

                // 요청보다 적으면 (다 먹었거나 남이 먼저 먹었거나)
                if (actuallyEaten < requestAmount)
                {
                    targetFood = null; // 파괴된 오브젝트의 좀비 참조를 완벽히 끊어버림!
                    entity.isEating = false; // 식사 끝!
                    SearchOrWander(entity); // 다시 배회 시작
                }
            }
        }
        // 2. 타겟 먹이가 아예 없거나, 먹으러 가는 도중에 누가 먹이를 치워버렸을 때 (Wander 중)
        else
        {
            // 혹시라도 남아있을 식사 상태를 강제로 꺼줌
            entity.isEating = false;

            if (isWandering && entity.IsArrivedAtTarget())
            {
                isWandering = false;
                entity.PlayAnimation("Idle"); // 목적지에 도착하면 멈춰서 두리번거리기
            }
        }
    }

    public void ExecuteTick(EntityController entity)
    {
        // 배가 가득 차면 Hunger 상태를 탈출
        if (entity.hunger >= entity.species.maxHunger)
        {
            entity.ChangeState(new IdleState());
            return;
        }

        // 먹이가 없거나, 향하던 먹이가 파괴된 상태라면 틱마다 레이더 가동
        if (targetFood == null || ((Component)targetFood) == null)
        {
            targetFood = entity.LookForFood();

            if (targetFood != null && ((Component)targetFood) != null)
            {
                isWandering = false;
                entity.isEating = false;

                Vector3 foodPosition = ((Component)targetFood).transform.position;
                entity.MoveTo(foodPosition);
                entity.PlayAnimation("Walk");
            }
            else if (!isWandering)
            {
                // 제자리에 서 있다면 50% 확률로 새로운 곳을 향해 배회 시작
                if (Random.value > 0.5f)
                {
                    SearchOrWander(entity);
                }
            }
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
    private float sleepRestoreSpeed;
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
        if (entity.energy >= entity.species.maxEnergy)
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