using UnityEngine;

public class SimpleFood : MonoBehaviour, IConsumable
{
    [SerializeField]
    public float maxNutrition = 100f;
    [SerializeField]
    private float currentNutrition;
    public float CurrentNutrition => currentNutrition;

    private void Awake()
    {
        currentNutrition = maxNutrition;
    }

    public float ConsumeNutrition(float requestAmount)
    {
        float eaten = Mathf.Min(currentNutrition, requestAmount);
        currentNutrition -= eaten;

        // 다 먹히면 파괴
        if (currentNutrition <= 0)
        {
            Destroy(gameObject);
        }

        return eaten; // 포식자에게 실제로 준 영양분 반환
    }
}