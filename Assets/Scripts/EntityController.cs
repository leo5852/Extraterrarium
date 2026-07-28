using UnityEngine;
using UnityEngine.AI;

public class EntityController : MonoBehaviour
{
    [Header("Species")]
    public SpeciesData species;

    [Header("Entity Stats (런타임 값, Start()에서 Species로 초기화)")]
    public float hunger;
    public float energy;
    public float eatSpeed;
    public float wanderRadius; // 배회 반경
    public float sleepRestoreSpeed; // 수면 중 에너지 회복 속도
    public bool isEating = false;
    public bool isSleeping = false;

    [Header("Detection")]
    public float searchRadius; // 먹이 탐지 반경

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

        if (species == null)
        {
            Debug.LogError($"{name}: SpeciesData가 할당되지 않음!");
        }
        else
        {
            hunger = species.maxHunger;
            energy = species.maxEnergy;
            eatSpeed = species.eatSpeed;
            wanderRadius = species.wanderRadius;
            sleepRestoreSpeed = species.sleepRestoreSpeed;
            searchRadius = species.searchRadius;
        }

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
        // 파괴/비활성화될 때 반드시 구독 해제
        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnTick -= ApplyTick;
        }
    }

    private void ApplyTick()
    {
        if (!isEating && !isSleeping) hunger -= species.hungerDropPerTick;
        if (!isSleeping) energy -= species.energyDropPerTick;
        hunger = Mathf.Clamp(hunger, 0, species.maxHunger);
        energy = Mathf.Clamp(energy, 0, species.maxEnergy);

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

    public void RestoreEnergy(float amount) { energy = Mathf.Min(energy + amount, species.maxEnergy); }
    public void RestoreHunger(float amount) { hunger = Mathf.Min(hunger + amount, species.maxHunger); }

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
        // 1. 내 위치를 기준으로 반경 내 무작위 방향 벡터 생성
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;

        NavMeshHit hit;
        // 2. 그 지점 근처의 실제 NavMesh 위 점을 탐색 (걸을 수 있는 영역인지 확인)
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // 유효한 점을 못 찾으면 현재 위치 반환 (제자리 유지)
        return transform.position;
    }
}

