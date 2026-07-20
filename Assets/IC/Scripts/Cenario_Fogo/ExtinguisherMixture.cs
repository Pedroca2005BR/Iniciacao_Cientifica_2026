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

    public MixtureType mixType;
    [SerializeField] private ParticleSystem mixtureParticle;
    [SerializeField] private float reduceFire = 0.1f;

    private FireParticlesType fire;

    private void Start()
    {
        if(mixtureParticle == null)
        {
            mixtureParticle = GetComponent<ParticleSystem>();
        }
    }

    public void PlayMixture(MixtureType type)
    {
        this.mixType = type;
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
            if ((mixType & MixtureType.A) != 0) return true;
        }
        else if ((fire.fireType & FireParticlesType.FireType.B) != 0)
        {
            if ((mixType & MixtureType.B) != 0) return true;
        }
        else if ((fire.fireType & FireParticlesType.FireType.C) != 0)
        {
            if ((mixType & MixtureType.C) != 0) return true;
        }
        return false;
    }

    private void OnParticleCollision(GameObject other)
    {
        var emission = other.GetComponent<ParticleSystem>().emission;
        float em = emission.rateOverTime.constant;
        fire = other.GetComponent<FireParticlesType>();
        if (other.CompareTag("Fire") && FireTypeTrue())
        {
            em -= reduceFire;
            Debug.Log(em);
            emission.rateOverTime = em;
            if(emission.rateOverTime.constant == 0f)
            {
                emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 0));
                fire.ActivateSmoke();
            }
            
        }
    }
}
