using System;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public static TickManager Instance;

    public float tickInterval = 1.0f;
    private float timer;

    public event Action OnTick;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tickInterval)
        {
            timer = 0f;

            // Invoke the OnTick event
            OnTick?.Invoke();
        }
    }
}
