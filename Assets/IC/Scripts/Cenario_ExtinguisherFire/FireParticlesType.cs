using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireParticlesType : MonoBehaviour
{
    [System.Flags]
    public enum FireType 
    { 
        A = 1,
        B = 2,
        C = 4
    }

    public FireType fireType;

    public GameObject smokeParticle;

    private void Awake()
    {
        smokeParticle = transform.Find("SmokeParticleSystem").gameObject;
    }

    public void ActivateSmoke()
    {
        smokeParticle.gameObject.SetActive(true);
    }

}
