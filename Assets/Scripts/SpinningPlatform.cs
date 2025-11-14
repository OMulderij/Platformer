using Unity.Mathematics;
using UnityEngine;

public class Spinningplatform : VelocityCalculator
{
    public float RotateSpeedX = 0f;
    public float RotateSpeedY = 30f;
    public float RotateSpeedZ = 0f;

    void Update()
    {
        transform.Rotate(new Vector3(RotateSpeedX, RotateSpeedY, RotateSpeedZ) * Time.deltaTime);
    }

    public override Vector3 GetVelocity(Transform playerPos)
    {
        Quaternion rotation = Quaternion.Euler(RotateSpeedX * Time.deltaTime, RotateSpeedY * Time.deltaTime, RotateSpeedZ * Time.deltaTime);
        Vector3 position = rotation * (playerPos.position - transform.position);

        return position - (playerPos.position - transform.position);
    }
}