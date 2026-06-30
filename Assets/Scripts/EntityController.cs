using UnityEngine;

public class EntityController : MonoBehaviour
{
    [Header("Entity Stats")]
    private float hunger = 100.0f;
    private float energy = 100.0f;

    [Header("Decrease Rate Settings")]
    [Range(0.0f, 100.0f)]
    private float hungerDecreaseRate = 2.0f;
    [Range(0.0f, 100.0f)]
    private float energyDecreaseRate = 1.5f;

    [SerializeField]
    private IState currentState;
    void Start()
    {
        ChangeState(new IdleState());
    }

    // Update is called once per frame
    void Update()
    {
        hunger -= hungerDecreaseRate * Time.deltaTime;
        energy -= energyDecreaseRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0.0f, 100.0f);
        energy = Mathf.Clamp(energy, 0.0f, 100.0f);

        currentState?.Execute(this);
    }

    void ChangeState(IState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);   
    }
}
