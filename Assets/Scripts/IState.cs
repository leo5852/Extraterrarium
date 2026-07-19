using UnityEngine;

// interface for state to ensure implementations of State methods
public interface IState
{
    void Enter(EntityController entity);
    
    // 프레임마다 실행: 부드러운 이동, 애니메이션, 실시간 식사(Time.deltaTime)
    void ExecuteFrame(EntityController entity);

    // 틱마다 실행: 수치 감소 계산, 무거운 판단(주변 적 탐색 등)
    void ExecuteTick(EntityController entity);
    void Exit(EntityController entity);
}
