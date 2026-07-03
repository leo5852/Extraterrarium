using UnityEngine;
using UnityEngine.AI;

public class EntityController : MonoBehaviour
{
    [Header("Entity Stats")]
    public float hunger = 100.0f;
    public float energy = 100.0f;

    [Header("Decrease Rate Settings")]
    [Range(0.0f, 100.0f)]
    private float hungerDecreaseRate = 2.0f;
    [Range(0.0f, 100.0f)]
    private float energyDecreaseRate = 1.5f;

    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    public AudioSource audioSource;

    [SerializeField]
    private IState currentState;

    public Transform currentFoodTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        //audioSource = GetComponent<AudioSource>();

        ChangeState(new IdleState());
    }

    // Update is called once per frame
    void Update()
    {
        DecreaseStats();
        currentState?.Execute(this);
    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);   
    }

    //===== Action Logics =====
    private void DecreaseStats()
    {
        hunger -= hungerDecreaseRate * Time.deltaTime;
        energy -= energyDecreaseRate * Time.deltaTime;
        hunger = Mathf.Clamp(hunger, 0.0f, 100.0f);
        energy = Mathf.Clamp(energy, 0.0f, 100.0f);
    }
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
        animator.SetBool("IsMoving", true);
    }

    public void ConsumeEnergy(float amount) { energy -= amount * Time.deltaTime; }
    public void RestoreEnergy(float amount) { energy += amount * Time.deltaTime; }
    public void RestoreHunger(float amount) { hunger += amount * Time.deltaTime; }

    public void PlayAnimation(string triggerName) { animator.SetTrigger(triggerName); }
    public void StopAnimation(string boolName) { animator.SetBool(boolName, false); }

    public bool FindFood()
    {
        Collider[] result = Physics.OverlapSphere(transform.position, 20f, LayerMask.GetMask("Food"));
        if(result.Length > 0)
        { 
            currentFoodTarget = result[0].transform;
            return true;
        }
        return false;
    }

    public void MoveToFoodTarget()
    {
        if(currentFoodTarget != null)
        {
            MoveTo(currentFoodTarget.position);
        }
    }

    public bool IsArrivedAtTarget()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    public void EatFoodTarget()
    {

    }
}

