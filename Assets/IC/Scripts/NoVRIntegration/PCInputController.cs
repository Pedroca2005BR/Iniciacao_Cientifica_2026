using Oculus.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;


namespace NoVRIntegration
{
    public class PCInputController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;

        private CharacterController _charController;
        private Transform _camera;
        
        public Vector3 movement {  get; private set; }



        private void Start()
        {
            _camera = Camera.main.transform;
            _charController = GetComponent<CharacterController>();
        }

        private void FixedUpdate()
        {
            Move();
        }


        void Move()
        {
            movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, _camera.eulerAngles.y, transform.eulerAngles.z);
            //Debug.Log($"Euler Angles : {transform.eulerAngles} - movement : {movement}");
            movement = transform.TransformDirection(movement);

            _charController.Move(_moveSpeed * Time.deltaTime * movement);
        }
    }
}
