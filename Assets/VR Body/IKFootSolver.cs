using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRBody
{
    public class IKFootSolver : MonoBehaviour
    {
        [HideInInspector] public bool isMovingForward;

        [Tooltip("The layer(s) considered as ground for the foot to step on.")]
        [SerializeField] LayerMask terrainLayer = default;
        [Tooltip("The transform representing the body or root of the character. The raycast will originate from this point.")]
        [SerializeField] Transform body = default;
        [Tooltip("The other foot's IKFootSolver component. This is used to ensure that both feet do not step at the same time, creating a more natural walking motion.")]
        [SerializeField] IKFootSolver otherFoot = default;
        [SerializeField] float speed = 4;
        [Tooltip("The distance the foot must be from the body's current position before it will attempt to step.")]
        [SerializeField] float stepDistance = .2f;
        [Tooltip("The length of the step. This determines how far forward or backwards the foot will move when stepping.")]
        [SerializeField] float stepLength = .2f;
        [SerializeField] float sideStepLength = .1f;

        [Tooltip("The height of the step. This determines how high the foot will lift off the ground during a step.")]
        [SerializeField] float stepHeight = .3f;
        [Tooltip("The offset of the foot from the raycast hit point. This can be used to adjust the foot's position to better align with the ground.")]
        [SerializeField] Vector3 footOffset = default;

        [Tooltip("The rotation offset of the foot. This can be used to adjust the foot's rotation to better align with the ground.")]
        [SerializeField] Vector3 footRotOffset = default;
        [SerializeField] float footYPosOffset = 0.1f;

        public float rayStartYOffset = 0;
        public float rayLength = 1.5f;

        float footSpacing;
        Vector3 oldPosition, currentPosition, newPosition;
        Vector3 oldNormal, currentNormal, newNormal;
        float lerp;

        private void Start()
        {
            footSpacing = transform.localPosition.x;
            currentPosition = newPosition = oldPosition = transform.position;
            currentNormal = newNormal = oldNormal = transform.up;
            lerp = 1;
        }

        // Update is called once per frame

        void Update()
        {
            transform.position = currentPosition + Vector3.up * footYPosOffset;
            transform.localRotation = Quaternion.Euler(footRotOffset);

            Ray ray = new Ray(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);

            Debug.DrawRay(body.position + (body.right * footSpacing) + Vector3.up * rayStartYOffset, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer.value))
            {
                if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
                {
                    lerp = 0;
                    Vector3 direction = Vector3.ProjectOnPlane(info.point - currentPosition, Vector3.up).normalized;

                    float angle = Vector3.Angle(body.forward, body.InverseTransformDirection(direction));

                    isMovingForward = angle < 50 || angle > 130;

                    if (isMovingForward)
                    {
                        newPosition = info.point + direction * stepLength + footOffset;
                        newNormal = info.normal;
                    }
                    else
                    {
                        newPosition = info.point + direction * sideStepLength + footOffset;
                        newNormal = info.normal;
                    }

                }
            }

            if (lerp < 1)
            {
                Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
                tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

                currentPosition = tempPosition;
                currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
                lerp += Time.deltaTime * speed;
            }
            else
            {
                oldPosition = newPosition;
                oldNormal = newNormal;
            }
        }

        private void OnDrawGizmos()
        {

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(newPosition, 0.1f);
        }



        public bool IsMoving()
        {
            return lerp < 1;
        }



    }
}