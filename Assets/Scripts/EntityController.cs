using UnityEditor;
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
    public float wanderChance;
    public float sleepRestoreSpeed; // 수면 중 에너지 회복 속도
    public bool isEating = false;
    public bool isSleeping = false;

    [Header("Detection")]
    public float searchRadius; // 먹이 탐지 반경

    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;
    public AudioSource audioSource;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    private Vector3 currentTarget;
    private bool hasTarget;

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
            wanderChance = species.wanderChance;
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
        // 파괴/비활성화될 때 반드시 tick 구독 해제
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
        // 1. 방향만 무작위로 선택하고, y축은 반영하지 않음
        Vector2 randomCircle = Random.insideUnitCircle.normalized * radius;
        Vector3 randomDirection = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        // 2. 그 지점 근처의 실제 NavMesh 위 점을 탐색 (걸을 수 있는 영역인지 확인)
        if (NavMesh.SamplePosition(randomDirection, out hit, 2f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // 유효한 점을 못 찾으면 현재 위치 반환 (제자리 유지)
        return transform.position;
    }

    public bool TryGetWanderDestination(float radius, out Vector3 destination, float minDistance = 0.5f)
    {
        destination = GetRandomNavMeshPoint(radius);
        return Vector3.Distance(destination, transform.position) >= minDistance;
    }

    //============== Action Logics ================
    public void MoveTo(Vector3 destination)
    {
        agent.SetDestination(destination);
        currentTarget = destination;
        hasTarget = true;
    }

    public void SetStoppingDistance(float distance)
    {
        agent.stoppingDistance = distance;
    }

    // 콜라이더가 있는 타겟을 향할 때 중심 말고 표면 근처 접근 지점을 반환
    public Vector3 GetApproachPosition(Collider targetCollider, float buffer = 0.3f)
    {
        Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
        Vector3 direction = (transform.position - closestPoint).normalized;
        if (direction.sqrMagnitude < 0.01f) direction = transform.forward; // 완전히 겹친 경우 방어

        return closestPoint + direction * buffer;
    }

    //==================Debugging Logics==================
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !hasTarget) return;

        // 목표 지점 X 표시 (실선, Gizmos)
        Gizmos.color = Color.red;
        float size = 0.5f;
        Gizmos.DrawLine(currentTarget + new Vector3(-size, 0.05f, -size), currentTarget + new Vector3(size, 0.1f, size));
        Gizmos.DrawLine(currentTarget + new Vector3(-size, 0.05f, size), currentTarget + new Vector3(size, 0.1f, -size));

        // 목표 지점까지 점선 (Handles)
        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawDottedLine(transform.position, currentTarget, 4f); // 4f은 점 간격
    }
#endif
}

