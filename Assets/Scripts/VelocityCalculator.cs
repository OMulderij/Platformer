using System;
using Unity.Mathematics;
using UnityEngine;

public class VelocityCalculator : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _velocity;
    private Spinningplatform spinningPlatform;

    private void Awake()
    {
        _previousPosition = transform.position;
        if (this.TryGetComponent<Spinningplatform>(out Spinningplatform platform))
        {
            spinningPlatform = platform;
        }
    }

    private void Update()
    {
        _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    public virtual Vector3 GetVelocity(Transform playerPos)
    {
        Vector3 positionDifference = _velocity * Time.deltaTime;
        if (spinningPlatform != null)
        {
            positionDifference += spinningPlatform.GetVelocity(playerPos);
        }
        return positionDifference;
    }
}