using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FollowPoint : MonoBehaviour
{
    private float currentCompletion = 0f;
    [SerializeField] private float timePerPoint = 5f;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform middlePoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ParticleSystem particles;
    private List<Transform> pointsToMoveTo = new List<Transform>();
    private int currentIndex = 0;

    void Awake()
    {
        particles.transform.position = startPoint.position;
        if (startPoint != null)
        {
            pointsToMoveTo.Add(startPoint);
        }
        if (middlePoint != null)
        {
            pointsToMoveTo.Add(middlePoint);
        }
        if (endPoint != null)
        {
            pointsToMoveTo.Add(endPoint);
        }
    }

    void Update()
    {
        currentCompletion += Time.deltaTime;
        ParticleSystem.EmissionModule em = particles.emission;
        em.enabled = true;

        if (particles.transform.position == pointsToMoveTo[currentIndex+1].position)
        {
            currentIndex++;
            currentCompletion = 0f;
        }

        if (currentIndex >= pointsToMoveTo.Count - 1)
        {
            em.enabled = false;
            currentIndex = 0;
            currentCompletion = 0f; 
        }

        // Debug.Log(pointsToMoveTo.Count);
        particles.transform.position = Vector3.Lerp(pointsToMoveTo[currentIndex].position, pointsToMoveTo[currentIndex+1].position, currentCompletion / timePerPoint);
    }
}