using UnityEngine;

// interface for state to ensure implementations of State methods
public interface IState
{
    void Enter(EntityController entity);
    void Execute(EntityController entity);
    void Exit(EntityController entity);
}
