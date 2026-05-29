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

    public FireExtinguisher fire;
    bool isActive = false;
    public float reduceFire = 0.1f;

    public void SetExtinguisherOn(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (isActive)
            {
                fire.Deactivate();
                isActive = false;
            }
            else
            {
                fire.Activate();
                isActive = true;
            }
        }
    }
}
