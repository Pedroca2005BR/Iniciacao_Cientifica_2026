using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Made this class similarly to AudioManager, to help understanding how to use it
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    [SerializeField] private SerializableDictionaryCell<string, ParticleSystem>[] particlePrefabs;

    // Used for pooling
    private Dictionary<string, Queue<ParticleSystem>> particlePool = new Dictionary<string, Queue<ParticleSystem>>();

    // Singleton pattern
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this function from the code you want using "ParticleManager.instance.PlayParticle"
    public ParticleSystem PlayParticle(string particleName, Vector3 position, Quaternion rotation)
    {
        if (!particlePool.ContainsKey(particleName))
        {
            particlePool[particleName] = new Queue<ParticleSystem>();
        }

        ParticleSystem particle;

        // If there's an instance of the particle in the pool, just calls it to the next location
        if (particlePool[particleName].Count > 0)
        {
            particle = particlePool[particleName].Dequeue();
            particle.transform.position = position;
            particle.transform.rotation = rotation;
            particle.gameObject.SetActive(true);
        }
        else
        {
            // If there's not an instance in the pool, instantiate a new instance
            SerializableDictionaryCell<string, ParticleSystem> entry = Array.Find(particlePrefabs, p => p.key == particleName);
            if (entry == null)
            {
                Debug.LogWarning($"Particle {particleName} not found!");
                return null;
            }

            particle = Instantiate(entry.value, position, rotation);
        }

        // Plays the particle to its fullest and returns it to the pool
        particle.Play();
        StartCoroutine(ReturnToPool(particle, particleName));

        // Returns the particle just in case more work is needed after creating it
        return particle;
    }

    // Makes the pooling system by storing the reference and calling it again next time its needed
    private IEnumerator ReturnToPool(ParticleSystem particle, string particleName)
    {
        yield return new WaitForSeconds(particle.main.duration + particle.main.startLifetime.constantMax);
        particle.gameObject.SetActive(false);
        particlePool[particleName].Enqueue(particle);
    }
}

