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
    private PlayerControl player;
    public LayerMask playerMask;

    public void StartTimer(PlayerControl controller)
    {
        player = controller;
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


            Vector3 spherePos = this.transform.forward * (Time.time - timeWhenSpawned) / animationLength * 4 + this.transform.position;
            bool foundPlayer = Physics.CheckSphere(spherePos, 1f, playerMask);

            if (foundPlayer)
            {
                player.ApplyForce(this.transform.rotation * new Vector3(1,1,1));
            }
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
