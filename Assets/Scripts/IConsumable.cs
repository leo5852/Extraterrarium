using UnityEngine;

public interface IConsumable
{
    float CurrentNutrition { get; }
    float ConsumeNutrition(float requestAmount);
}
