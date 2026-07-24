using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimator : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 originalScale;

    [Header("Breathing(Idle)")]
    public float breatheSpeed = 2f;
    public float breatheAmount = 0.1f;

    [Header("Jumping")]
    public float jumpSpeed = 10f;
    public float jumpHeight = 0.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 1. 현재 이동 중인지 확인 (속도가 0.1보다 크면 이동 중)
        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            // [이동 중]: 통통 튀며 앞으로 감
            // Sin 함수로 0~1 사이의 포물선 값을 만듦
            float jumpWave = Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed));

            // Y축으로 길쭉해지면서 통통 튀는 효과
            transform.localScale = originalScale + new Vector3(-jumpWave * 0.2f, jumpWave * 0.5f, -jumpWave * 0.2f);

            // 몸을 살짝 띄움
            transform.localPosition = new Vector3(0, jumpWave * jumpHeight, 0);
        }
        else
        {
            // [대기 중]: 제자리에서 숨을 쉼
            // Sin 함수로 -1~1 사이의 파동을 만듦
            float breatheWave = Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;

            // 숨을 들이마시면 위로 살짝 길어지고, 내쉬면 옆으로 퍼지는 찰흙 느낌!
            transform.localScale = originalScale + new Vector3(-breatheWave, breatheWave, -breatheWave);

            // 바닥에 붙어있도록 위치 초기화
            transform.localPosition = Vector3.zero;
        }
    }
}
