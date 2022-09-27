using UnityEngine;
using UnityEngine.Events;

public class HandTrackingDiamond : MonoBehaviour
{
    [SerializeField] Transform righttIndexFingerTip;
    private float gestureCooldown = 0.5f;
    private bool onCooldown;
    private float cooldownTimer = 0;
    private float detectTimer = 0;

    [SerializeField] Transform centerEyeAnchor;
    public Transform GesturePosition;

    [HideInInspector] public bool IsShowing;
    public UnityEvent OnTeleportGesture;
    private bool isTeleportGesture;

    void Update()
    {   
        //If handtracking enabled
        if (OVRInput.IsControllerConnected(OVRInput.Controller.Hands))
        {
            if (onCooldown)
            {
                HideGesture();
                cooldownTimer += Time.deltaTime;
                if (cooldownTimer >= gestureCooldown)
                {
                    cooldownTimer = 0;
                    onCooldown = false;
                }
            }
            else
            {
                if (isTeleportGesture == true) 
                {
                    IsShowing = true;
                   GesturePosition.transform.position = righttIndexFingerTip.position;
                   GesturePosition.transform.forward = GesturePosition.transform.position - centerEyeAnchor.transform.position;
                    detectTimer += Time.deltaTime;
                    if (detectTimer > 2.0f)
                    { 
                            OnTeleportGesture.Invoke();
                            onCooldown = true;
                            HideGesture();
                        detectTimer = 0;
                    }
                }
                else
                {
                    HideGesture();
                }
            }
        }
    }

    private void HideGesture()
    {
        GesturePosition.transform.position = new Vector3(0, -10, 0);
        IsShowing = false;
    }
    public void RightGesture()
    {
        isTeleportGesture = true;
    }   
    public void WrongGesture()
    {
        isTeleportGesture = false;
    }
}