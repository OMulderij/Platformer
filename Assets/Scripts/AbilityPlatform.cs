using System;
using UnityEngine;

public class AbilityPlatform : MonoBehaviour
{
    public float lifetime = 10f;
    public float animationLength = 0.5f;
    public GameObject platform;
    public ParticleSystem spawnParticles;
    public ParticleSystem destroyParticles;
    private float timeWhenSpawned = 0.0f;
    private bool animationPlayed = false;

    public void StartTimer()
    {
        timeWhenSpawned = Time.time;
        this.transform.localScale = new Vector3(1, 1, 0);

        spawnParticles.Play();

        Destroy(platform, lifetime);
        Destroy(gameObject, lifetime + animationLength);
    }

    public void FixedUpdate()
    {
        if (timeWhenSpawned == 0.0f)
        {
            return;
        }
        
        if (Time.time - timeWhenSpawned < animationLength)
        {
            Vector3 currentScale = new Vector3(1, 1, Mathf.Lerp(0, 1, (Time.time - timeWhenSpawned) / animationLength));
            this.transform.localScale = currentScale;
        }
        else
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }

        if (!animationPlayed && Time.time - timeWhenSpawned > lifetime - animationLength / 2)
        {
            destroyParticles.Play();
            animationPlayed = true;
        }
    }
}
