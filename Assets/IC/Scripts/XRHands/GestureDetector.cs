using Oculus.Interaction.Locomotion;
using System;
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

        private float[] fullCurls = new float[5];
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

            // For each finger, find the desired curl value for FullCurl and store it in the fullCurls array
            for (int j = 0; j < 5; j++)
            {
                if (args.hand.CalculateFingerShape((XRHandFingerID)j, XRFingerShapeTypes.FullCurl).TryGetFullCurl(out float curl))
                {
                    fullCurls[j] = curl;
                }

                //Debug.Log($"Finger: {(XRHandFingerID)j} | Full Curl: {fullCurls[j]}");
            }



            foreach (var handShape in handShapes)
            {
                if (!completenessCalculator.TryCalculateHandShapeCompletenessScore(args.hand, handShape, out float completenessScore))
                {
                    Debug.Log($"Failed to calculate completeness score for gesture: {handShape.name}");
                    continue;
                }
                if (completenessScore >= confidenceThreshold)
                {
                    
                    


                    Debug.Log($"Detected gesture: {handShape.name} | Confidence: {completenessScore} | Full Curls: {string.Join(", ", fullCurls)}");
                    // You can trigger events or actions based on the detected gesture here



                    if (handShape.name == startTeleportGesture && fullCurls[1] < 0.3f)
                    {
                        //Debug.Log("Teleport gesture detected. Initiating teleportation sequence.");
                        OnStartTeleport();
                        break;
                    }
                    else if (handShape.name == confirmGestureName && !_isTeleporting && _isRayActive)
                    {
                        ConfirmTeleport();
                        break;
                    }
                    else
                    {
                        //Debug.Log("Cancel teleport gesture detected. Cancelling teleportation sequence.");
                        OnCancelTeleport();
                        break;
                    }

                    // At the end of the events, break the loop to avoid multiple gestures being detected in the same frame
                    
                }
            }

            _lastCheckTime = Time.time;
        }







        // Testing Input Calls with hand gesture detection
        [Header("Teleport Test")]
        [SerializeField] XRRayInteractor m_TeleportInteractor;
        [SerializeField] TeleportationProvider teleportProvider;
        [SerializeField] string startTeleportGesture; // Name of the hand gesture to trigger teleportation
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
            //Debug.Log(Environment.StackTrace);

            if (m_TeleportInteractor.TryGetCurrent3DRaycastHit(out var hit) && !_isTeleporting)
            {
                //Debug.Log("Say Cheese!");

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