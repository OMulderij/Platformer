using Unity.Mathematics;
using UnityEngine;

public class Spinningplatform : MonoBehaviour
{
    public float RotateSpeedX = 0f;
    public float RotateSpeedY = 30f;
    public float RotateSpeedZ = 0f;

    void Update()
    {
        transform.Rotate(new Vector3(RotateSpeedX, RotateSpeedY, RotateSpeedZ) * Time.deltaTime);
    }
}