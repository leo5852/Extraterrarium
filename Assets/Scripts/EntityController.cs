using UnityEngine;
using UnityEngine.AI;

public class EntityController : MonoBehaviour
{
    [Header("Entity Stats")]
    public float hunger = 100.0f;
    public float energy = 100.0f;
    public float eatSpeed = 5.0f; // 먹는 속도  
    public float wanderRadius = 5f; // 배회 반경
    public float sleepRestoreSpeed = 15f; // 잠자는 동안 회복 속도
    public bool isEating = false;
    public bool isSleeping = false;

    [Header("Detection")]
    public float searchRadius = 10f; // 먹이 탐색 반경

    [Header("Drop Per Tick")]
    [Range(0.0f, 100.0f)]
    private float hungerDropPerTick = 5.0f;
    [Range(0.0f, 100.0f)]
    private float energyDropPerTick = 1.5f;


    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    public AudioSource audioSource;

    [SerializeField]
    private IState currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //animator = GetComponent<Animator>();
        //audioSource = GetComponent<AudioSource>();

        ChangeState(new IdleState());

        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnTick += ApplyTick;
        }
        else
        {
            Debug.LogError("Can't find TickManager! Tick not working...");
        }
    }

    private void OnDisable()
    {
        // 파괴/비활성화될 때 반드시 구독 취소
        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnTick -= ApplyTick;
        }
    }

    private void ApplyTick()
    {
        if (!isEating && !isSleeping) hunger -= hungerDropPerTick;
        if (!isSleeping) energy -= energyDropPerTick;
        hunger = Mathf.Max(hunger, 0);
        energy = Mathf.Max(energy, 0);

        if (currentState != null)
        {
            currentState.ExecuteTick(this);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (currentState != null)
        {
            currentState?.ExecuteFrame(this);
        }
    }

    public void ChangeState(IState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);   
    }

    //==== Action Logics ====
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
    }

    public void RestoreEnergy(float amount) { energy += amount; }
    public void RestoreHunger(float amount) { hunger += amount; }

    public void PlayAnimation(string triggerName) { animator.SetTrigger(triggerName); }
    public void StopAnimation(string boolName) { animator.SetBool(boolName, false); }

    public IConsumable LookForFood()
    {
        Collider[] hitColls = Physics.OverlapSphere(transform.position, searchRadius, LayerMask.GetMask("Food"));
        float closestDistance = Mathf.Infinity;
        IConsumable closestConsumable = null;

        foreach (Collider coll in hitColls)
        {
            IConsumable foundConsumable = coll.GetComponent<IConsumable>();
            if (foundConsumable != null)
            {
                float distance = Vector3.Distance(transform.position, coll.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestConsumable = foundConsumable;
                }
            }
        }
        if (closestConsumable != null) return closestConsumable;
        else return null;
    }

    public bool IsArrivedAtTarget()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector3 GetRandomNavMeshPoint(float radius)
    {
        // 1. 내 위치를 기준으로 반경 내의 랜덤한 방향 벡터 생성
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit hit;
        // 2. 그 방향과 가장 가까운 NavMesh 위의 점을 찾음 (갈 수 있는 길인지 확인)
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // 길을 못 찾았으면 일단 현재 위치 반환 (제자리 걸음)
        return transform.position;
    }
}

