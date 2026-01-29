using UnityEngine;

public class ExtinguisherMixture : MonoBehaviour
{
    public enum MixtureType
    {
        A = 0,
        AB = 1,
        BC = 2,
        ABC = 3
    }

    public MixtureType type;
    [SerializeField] private ParticleSystem mixtureParticle;

    private void Start()
    {
        if(mixtureParticle == null)
        {
            mixtureParticle = GetComponent<ParticleSystem>();
        }
    }

    public void PlayMixture(MixtureType type)
    {
        this.type = type;
        mixtureParticle.Play();
    }
    
    public void StopMixture()
    {
        mixtureParticle.Stop();
    }



    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Fire"))
        {
            other.GetComponent<ParticleSystem>().Stop();
        }
    }
}
