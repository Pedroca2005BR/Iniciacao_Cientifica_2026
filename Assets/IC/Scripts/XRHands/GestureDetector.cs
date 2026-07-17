using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Hands.Samples.GestureSample;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

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

        #endregion

        // Testing method to detect gestures based on hand joint data
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


                    if (handShape.name == handGestureName)
                    {
                        Debug.Log("Teleport gesture detected. Initiating teleportation sequence.");
                        OnStartTeleport();
                    }
                    else if (handShape.name == confirmGestureName)
                    {
                        if (!_isTeleporting)
                        {
                            Debug.Log("Confirm teleport gesture detected. Confirming teleportation.");
                            ConfirmTeleport();
                        }
                    }
                    else if (handShape.name == cancelGestureName)
                    {
                        Debug.Log("Cancel teleport gesture detected. Cancelling teleportation sequence.");
                        OnCancelTeleport();
                    }

                    // At the end of the events, break the loop to avoid multiple gestures being detected in the same frame
                    break;
                }
            }

            _lastCheckTime = Time.time;
        }







        // Testing Input Calls with hand gesture detection
        [Header("Teleport Test")]
        [SerializeField] XRRayInteractor m_TeleportInteractor;
        [SerializeField] TeleportationProvider teleportProvider;
        [SerializeField] string handGestureName; // Name of the hand gesture to trigger teleportation
        [SerializeField] string cancelGestureName; // Name of the hand gesture to cancel teleportation
        [SerializeField] string confirmGestureName; // Name of the hand gesture to confirm teleportation
        private bool m_PostponedDeactivateTeleport;
        bool _isTeleporting = false;
        bool _isRayActive = false;  // Controls if the teleport ray is active or not


        void OnStartTeleport()
        {
            if (_isRayActive)
                return;

            m_PostponedDeactivateTeleport = false;

            if (m_TeleportInteractor != null)
            {
                m_TeleportInteractor.gameObject.SetActive(true);
                _isRayActive = true;
            }

            //if (m_RayInteractor != null)
            //    m_RayInteractor.gameObject.SetActive(false);


            //m_RayInteractorChanged?.Invoke(m_TeleportInteractor);
        }

        void OnCancelTeleport()
        {
            // Do not deactivate the teleport interactor in this callback.
            // We delay turning off the teleport interactor in this callback so that
            // the teleport interactor has a chance to complete the teleport if needed.
            // OnAfterInteractionEvents will handle deactivating its GameObject.
            m_PostponedDeactivateTeleport = true;

            //if (m_RayInteractor != null)
            //    m_RayInteractor.gameObject.SetActive(true);


            //m_RayInteractorChanged?.Invoke(m_RayInteractor);
        }

        protected void Update()
        {
            // Since this behavior has the default execution order, it runs after the XRInteractionManager,
            // so selection events have been finished by now this frame. This means that the teleport interactor
            // has had a chance to process its select interaction event and teleport if needed.
            if (m_PostponedDeactivateTeleport)
            {
                if (m_TeleportInteractor != null)
                    m_TeleportInteractor.gameObject.SetActive(false);

                m_PostponedDeactivateTeleport = false;
                _isTeleporting = false;
                _isRayActive = false;
            }

        }

        

        public void ConfirmTeleport()
        {
            if (m_TeleportInteractor.TryGetCurrent3DRaycastHit(out var hit) && !_isTeleporting)
            {
                _isTeleporting = true;
                TeleportRequest request = new TeleportRequest
                {
                    destinationPosition = hit.point,
                    destinationRotation = Quaternion.identity
                };

                teleportProvider.QueueTeleportRequest(request);

                // Cancels the teleportation sequence after confirming the teleportation.
                OnCancelTeleport();
            }
        }
    }
}