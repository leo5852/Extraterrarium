using UnityEngine;

// Two Bone IK Constraint의 Target 오브젝트를 절차적으로 움직여 걷는 모션을 만듦.
// 애니메이션 클립 없이도 동작함. 다리마다 하나씩 붙임 (예: RightLegIK_target에 부착).
public class LegStepper : MonoBehaviour
{
    [Header("기준점")]
    [Tooltip("몸통을 따라다니는, 이 발이 있어야 할 이상적인 위치 (RightFootAnchor 등)")]
    public Transform footAnchor;
    [Tooltip("같은 개체의 반대쪽 다리 LegStepper. 동시에 스텝하지 않도록 서로 체크함")]
    public LegStepper otherLeg;

    [Header("지면 감지")]
    public LayerMask groundLayer;
    public float raycastStartHeight = 0.5f;
    public float raycastDownDistance = 1.5f;

    [Header("스텝 조건")]
    [Tooltip("anchor와 현재 발 위치가 이 거리 이상 벌어지면 스텝 시작")]
    public float stepThreshold = 0.4f;
    public float stepDuration = 0.25f;
    public float stepArcHeight = 0.15f;

    private bool isStepping;
    private float stepTimer;
    private Vector3 stepStartPos;
    private Vector3 stepEndPos;

    void Start()
    {
        // 시작할 때 바로 anchor 위치로 세팅 (첫 프레임에 다리가 이상한 곳에서 시작하지 않도록)
        transform.position = GetGroundedAnchorPosition();
    }

    void Update()
    {
        if (isStepping)
        {
            UpdateStep();
            return;
        }

        Vector3 desiredPos = GetGroundedAnchorPosition();
        float distance = Vector3.Distance(transform.position, desiredPos);

        if (distance > stepThreshold && CanStartStep())
        {
            StartStep(desiredPos);
        }
    }

    private bool CanStartStep()
    {
        // 반대쪽 다리가 이미 스텝 중이면 이번엔 양보 (다음 프레임에 재시도)
        return otherLeg == null || !otherLeg.IsStepping;
    }

    private void StartStep(Vector3 targetPos)
    {
        isStepping = true;
        stepTimer = 0f;
        stepStartPos = transform.position;
        stepEndPos = targetPos;
    }

    private void UpdateStep()
    {
        stepTimer += Time.deltaTime;
        float t = Mathf.Clamp01(stepTimer / stepDuration);

        Vector3 pos = Vector3.Lerp(stepStartPos, stepEndPos, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * stepArcHeight; // 스텝 중간에 살짝 들렸다가 내려오는 포물선
        transform.position = pos;

        if (t >= 1f)
        {
            isStepping = false;
        }
    }

    private Vector3 GetGroundedAnchorPosition()
    {
        Vector3 rayOrigin = footAnchor.position + Vector3.up * raycastStartHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastStartHeight + raycastDownDistance, groundLayer))
        {
            return hit.point;
        }
        return footAnchor.position;
    }

    public bool IsStepping => isStepping;
}