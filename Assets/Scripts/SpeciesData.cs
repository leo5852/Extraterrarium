using UnityEngine;

/// <summary>
/// 종(species)별 고유 스탯과 생태 관계를 담는 데이터 에셋.
/// 새 종을 추가할 때 스크립트를 건드리지 않고 이 에셋만 새로 만들면 됨.
/// </summary>
[CreateAssetMenu(fileName = "New SpeciesData", menuName = "Extraterrarium/Species Data")]
public class SpeciesData : ScriptableObject
{
    [Header("Identity")]
    public string speciesName;

    [Header("Needs (최대치 / 틱당 감소량)")]
    public float maxHunger = 100f;
    public float maxEnergy = 100f;
    public float hungerDropPerTick = 5f;
    public float energyDropPerTick = 1.5f;

    [Header("State Thresholds (이 값 밑으로 떨어지면 상태 전환)")]
    public float hungerTrigger = 60f; // 이 밑이면 HungerState로 전환
    public float energyTrigger = 10f; // 이 밑이면 SleepState로 전환

    [Header("Movement & Senses")]
    public float wanderRadius = 5f;
    public float searchRadius = 10f;

    [Header("Eating & Resting")]
    public float eatSpeed = 5f;
    public float sleepRestoreSpeed = 15f;

    [Header("Ecosystem Relations")]
    [Tooltip("이 종이 사냥/섭취할 수 있는 먹이 종 목록. 추후 HuntState에서 사용 예정.")]
    public SpeciesData[] preySpecies;
}
