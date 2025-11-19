using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private bool goingForwards = true;
    private float currentCompletion = 0f;
    [SerializeField] private float timeToFinish = 5f;

    public Transform startPoint;
    public Transform endPoint;
    [SerializeField] private GameObject platform;

    void Awake()
    {
        platform.transform.position = startPoint.position;
    }

    void Update()
    {
        if (goingForwards)
        {
            currentCompletion += Time.deltaTime;

            if (currentCompletion >= timeToFinish)
            {
                goingForwards = false;
            }
        }
        else
        {
            currentCompletion -= Time.deltaTime;

            if (currentCompletion <= 0)
            {
                goingForwards = true;
            }
        }

        platform.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, currentCompletion / timeToFinish);
    }
}