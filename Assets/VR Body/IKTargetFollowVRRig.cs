using UnityEngine;

namespace VRBody
{
    [System.Serializable]
    public class VRMap
    {
        public Transform vrTarget;
        public Transform ikTarget;
        public Vector3 trackingPositionOffset;
        public Vector3 trackingRotationOffset;
        public void Map()
        {
            ikTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
            ikTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
        }
    }

    public class IKTargetFollowVRRig : MonoBehaviour
    {
        [Range(0, 1)]
        public float turnSmoothness = 0.1f;
        public VRMap head;
        public VRMap leftHand;
        public VRMap rightHand;

        public float headBodyPositionOffsetY = -0.6f;
        public float camDistanceFromHead = 0.5f;



        // Update is called once per frame
        void LateUpdate()
        {
            transform.localPosition = head.ikTarget.position + GetNewCamPosition();
            float yaw = head.vrTarget.eulerAngles.y;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.eulerAngles.x, yaw, transform.eulerAngles.z), turnSmoothness);

            head.Map();
            leftHand.Map();
            rightHand.Map();
        }

        Vector3 GetNewCamPosition()
        {
            float rad = head.vrTarget.eulerAngles.y * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad);
            float z = Mathf.Cos(rad);
            Vector3 pos = new Vector3(x, 0, z) * camDistanceFromHead;
            pos.y = headBodyPositionOffsetY;
            return pos;
        }


    }
}