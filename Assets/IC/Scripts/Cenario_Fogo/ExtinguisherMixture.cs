using UnityEngine;

public class ExtinguisherMixture : MonoBehaviour
{
    [System.Flags]
    public enum MixtureType
    {
        A = 1,
        B = 2,
        C = 4
    }

    public MixtureType type;
    [SerializeField] private ParticleSystem mixtureParticle;


    public FireParticlesType fire;

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

    public bool FireTypeTrue()
    {
        if((fire.fireType & FireParticlesType.FireType.A) != 0)
        {
            if ((type & MixtureType.A) != 0) return true;
        }
        else if ((fire.fireType & FireParticlesType.FireType.B) != 0)
        {
            if ((type & MixtureType.B) != 0) return true;
        }
        else if ((fire.fireType & FireParticlesType.FireType.C) != 0)
        {
            if ((type & MixtureType.C) != 0) return true;
        }
        return false;
    }

    private void OnParticleCollision(GameObject other)
    {
        var emission = other.GetComponent<ParticleSystem>().emission;
        float em = emission.rateOverTime.constant;
        if (other.CompareTag("Fire") && FireTypeTrue())
        {
            em -= fire.reduceFire;
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
