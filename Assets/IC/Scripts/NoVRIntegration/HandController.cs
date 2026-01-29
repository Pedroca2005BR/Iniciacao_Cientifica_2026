using UnityEngine;
using UnityEngine.InputSystem;


namespace NoVRIntegration
{
    public class HandController : MonoBehaviour
    {
        GameObject _objectOnHold = null;
        [SerializeField] private float _interactionRange;

        private void Update()
        {
            if (_objectOnHold != null)
            {
                TranslateObject();
            }
        }

        private void TranslateObject()
        {
            _objectOnHold.transform.position = transform.position;
            _objectOnHold.transform.rotation = transform.rotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Tá funfando!");
            if (other.transform.parent.parent.TryGetComponent<IInteractable>(out IInteractable component))
            {
                Debug.Log("Pq n ta na mao???!");

                // Just for this. Change later
                _objectOnHold = component.GetObject();
                component.Activate();
            }
            else
            {
                Debug.Log("N tem Interactable n, amigo!");

            }
        }

        private void OnAttack(InputValue inputValue)
        {
            if (_objectOnHold != null)
            {
                _objectOnHold.GetComponent<IInteractable>().Deselect();
            }
        }

        private void OnJump(InputValue inputValue)
        {

        }
    }
}
