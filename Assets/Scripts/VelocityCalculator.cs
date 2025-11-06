using System;
using Unity.Mathematics;
using UnityEngine;

public class VelocityCalculator : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _velocity;
    public Spinningplatform platform;

    private void Start()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    public Vector3 GetVelocity(Transform playerPos)
    {
        Vector3 positionDifference = _velocity * Time.deltaTime;
        if (platform != null)
        {
            Quaternion rotation = Quaternion.Euler(platform.RotateSpeedX * Time.deltaTime, platform.RotateSpeedY * Time.deltaTime, platform.RotateSpeedZ * Time.deltaTime);
            Vector3 position = rotation * (playerPos.position - platform.transform.position);

            positionDifference += position - (playerPos.position - platform.transform.position);
        }
        return positionDifference;
    }
}