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


    public ExtinguisherTest extTst;

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
        var emission = other.GetComponent<ParticleSystem>().emission;
        float em = emission.rateOverTime.constant;
        if (other.CompareTag("Fire"))
        {
            em -= extTst.reduceFire;
            Debug.Log(em);
            emission.rateOverTime = em;
            if(emission.rateOverTime.constant == 0f)
            {
                emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 0));
            }
            //other.GetComponent<ParticleSystem>().Stop();
        }
    }
}
