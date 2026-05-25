using UnityEngine;
using UnityEngine.InputSystem;

public class ExtinguisherTest : MonoBehaviour
{

    public FireExtinguisher fire;
    bool isActive = false;
    public float reduceFire = 0.1f;

    public void LigarInsintor(InputAction.CallbackContext context)
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
