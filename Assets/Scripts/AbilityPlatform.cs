using UnityEngine;

public class AbilityPlatform : MonoBehaviour
{
    public float lifetime = 10f;
    public float animationLength = 1f;
    private float timeWhenSpawned = 0.0f;
    private bool spawned = false;

    public void StartTimer()
    {
        timeWhenSpawned = Time.time;
        spawned = true;
    }

    public void FixedUpdate()
    {
        if (spawned && Time.time - timeWhenSpawned > lifetime)
        {
            Destroy(gameObject);
        }
    }
}
