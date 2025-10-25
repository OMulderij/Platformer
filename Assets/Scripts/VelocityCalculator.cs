using System;
using Unity.Mathematics;
using UnityEngine;

public class VelocityCalculator : MonoBehaviour
{
    private Vector3 _previousPosition;
    private Vector3 _velocity;
    public Spinningplatform platform;

    private Quaternion lastPlatFormRotation;


    float r, angle = 1;
    Vector3 startpos;

    private void Start()
    {
        _previousPosition = transform.position;
        angle = 0;
        startpos = transform.position;
    }

    private void Update()
    {
        _velocity = (transform.position - _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
    }

    // player script gets the platform's velocity from here
    public Vector3 GetVelocity(Transform playerPos)
    {
        if (platform != null)
        {
            Quaternion rotation = Quaternion.Euler(platform.RotateSpeedX, platform.RotateSpeedY, platform.RotateSpeedZ);
            Vector3 position = rotation * playerPos.position;

            // * 1.06f and * 0.93f are what I found to be the most accurate rotations through bruteforce testing.
            Vector3 positionDifference = position * 1.06f - playerPos.position * 0.93f;
            // Debug.Log(positionDifference);
            return positionDifference;
        }
        return _velocity;
    }

    public static Vector3 RotateFromPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return pivot + (rotation * (point - pivot));
    }
}