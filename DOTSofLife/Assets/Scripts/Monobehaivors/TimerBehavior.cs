using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerBehavior : MonoBehaviour
{
    public float totalTime = 3f;
    private float currentTime;
    public bool isActive = false;

    void Start()
    {
        currentTime = totalTime;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            ResetTimer();
        }

        if (isActive)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                isActive = false;
            }
        }
    }

    void ResetTimer()
    {
        currentTime = totalTime;
        isActive = true;
    }
}
