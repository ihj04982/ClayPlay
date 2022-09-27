using UnityEngine;

namespace Chiligames.MetaAvatarsPun
{
    public class Locomotion : MonoBehaviour
    {
        // [Header("Continuous Movemet")]
        //[SerializeField] bool ContinuousMovementEnabled;
        //[SerializeField] float continuousMovementSpeed = 1;
        //[SerializeField] float snapTurnRotationAngle = 45f;
        //[SerializeField] float snapTurnCooldown = 0.5f;
        private bool snapTurnOnCooldown;
        private float snapTurnCounter;
        //Which controller thumbstick will be used for controlling movement
        //[SerializeField] OVRInput.Controller continuousMovementController = OVRInput.Controller.LTouch;
        //Which controller thumbstick will be used for controlling snap rotation
        //[SerializeField] OVRInput.Controller snapTurnController = OVRInput.Controller.RTouch;

        [Header("Teleportation")]
        [SerializeField] bool TeleportingEnabled;
        //The mesh that represents the point to teleport
        [SerializeField] MeshRenderer teleportPoint;
        //Which controller thumbstick will be able to trigger teleporting
        //   [SerializeField] OVRInput.Controller teleportController = OVRInput.Controller.LTouch;
        //  [SerializeField] OVRInput.Button buttonForTeleportation = OVRInput.Button.PrimaryThumbstickUp;
        //The points from where the teleport ray starts. 0 is left hand, 1 is right hand.
        [SerializeField] private Transform[] teleportingStartPointControllers;
        [SerializeField] private HandTrackingDiamond handTrackingDiamondGesture;
        private bool ableToTeleport;
        // private bool teleportWasPressedLastFrame;
        //The layer where teleporting is permitted
        [SerializeField] private LayerMask teleportLayermask;

        [Header("References")]
        [SerializeField] Transform OVRRig;
        // [SerializeField] Transform centerEyeAnchor;
        [SerializeField] private CapsuleCollider bodyCollider;
        [SerializeField] private Rigidbody rigidBody;
        [SerializeField] CurvedLaser curvedLaser;


        private bool isMoving;
        private Vector3 tPoint;

        private void Awake()
        {
            rigidBody.isKinematic = true;
            bodyCollider.isTrigger = true;
            handTrackingDiamondGesture.OnTeleportGesture.AddListener(Teleport);
        }

        void Update()
        {
            if (TeleportingEnabled)
            {
                CheckIfTeleportButton();
            }
        }

        private void CheckIfTeleportButton()
        {
            //If handtracking active
            if (OVRInput.IsControllerConnected(OVRInput.Controller.Hands))
            {
                if (handTrackingDiamondGesture.IsShowing)
                {
                    ShowPointerHandTracking(handTrackingDiamondGesture.GesturePosition.position, handTrackingDiamondGesture.GesturePosition.forward);
                }
                else
                {
                    HidePointer();
                    ableToTeleport = false;
                }
            }
        }

        //Hide pointer target and curve
        private void HidePointer()
        {
            curvedLaser.Activate(false);
            teleportPoint.enabled = false;
        }

        //Show the point target when using hand tracking
        private void ShowPointerHandTracking(Vector3 _startPoint, Vector3 _direction)
        {
            RaycastHit hit;

            if (Physics.Raycast(_startPoint, _direction, out hit, 10))
            {
                if (teleportPoint.enabled == false)
                {
                    teleportPoint.enabled = true;
                    teleportPoint.transform.position = hit.point;
                }
                var _teleportPoint = new Vector3(hit.point.x, 0, hit.point.z);
                teleportPoint.transform.position = Vector3.Lerp(teleportPoint.transform.position , _teleportPoint, 0.1f);

                if (((1 << hit.collider.gameObject.layer) & teleportLayermask) != 0)
                {
                    teleportPoint.material.color = Color.green;
                    ableToTeleport = true;
                }
                else
                {
                    teleportPoint.material.color = Color.red;
                    ableToTeleport = false;
                }
            }
        }

        //Teleport to teleport point destination
        private void Teleport()
        {
            if (ableToTeleport)
            {
                OVRRig.transform.position = teleportPoint.transform.position;
            }
        }

        private void OnDestroy()
        {
            handTrackingDiamondGesture.OnTeleportGesture.RemoveListener(Teleport);
        }
    }
}
