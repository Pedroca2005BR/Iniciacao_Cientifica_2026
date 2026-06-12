using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.GestureSample;

namespace IC.XRHands
{
    public class GestureDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] XRHandTrackingEvents _xrHandTrackingEvents;
        [SerializeField] HandShapeCompletenessCalculator completenessCalculator;

        [Header("Gesture Detection Settings")]
        [SerializeField] XRHandShape[] handShapes;
        [SerializeField] float confidenceThreshold = 0.9f;
        [SerializeField] float checkInterval = 0.1f;
        
        private float _lastCheckTime;

        #region Event Subscriptions

        private void OnEnable()
        {
            _xrHandTrackingEvents.jointsUpdated.AddListener(OnJointsUpdated);
        }

        private void OnDisable()
        {
            _xrHandTrackingEvents.jointsUpdated.RemoveListener(OnJointsUpdated);
        }

        void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            if (Time.time - _lastCheckTime < checkInterval) return;


            foreach (var handShape in handShapes)
            {
                if (!completenessCalculator.TryCalculateHandShapeCompletenessScore(args.hand, handShape, out float completenessScore))
                {
                    Debug.Log($"Failed to calculate completeness score for gesture: {handShape.name}");
                    continue;
                }
                if (completenessScore >= confidenceThreshold)
                {
                    Debug.Log($"Detected gesture: {handShape.name} | Confidence: {completenessScore}");
                    // You can trigger events or actions based on the detected gesture here
                }
            }

            _lastCheckTime = Time.time;
        }

        #endregion
    }
}