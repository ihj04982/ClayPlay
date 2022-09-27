using Oculus.Avatar2;
using Oculus.Platform;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;
//
//For handling local objects and sending data over the network
//
namespace Chiligames.MetaAvatarsPun
{
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] GameObject OVRCameraRig;
        [SerializeField] Transform centerEyeAnchor;
        [SerializeField] Transform[] spawnPoints;

        [SerializeField] GameObject avatarPrefab;
        [SerializeField] GameObject avatarTestPrefab;

        [SerializeField] GameObject photonVoiceSetupPrefab;

        [HideInInspector] public ulong userID = 0;

        bool hasJoinedRoom = false;
        bool userIsEntitled = false;

        private GameObject myAvatarEntity;

        [Tooltip("Enable this to use predefined avatars, doesn't require Oculus authentication")]
        [SerializeField] bool noOculusCredentials = false;

        private void Awake()
        {
            //For testing without credentials in editor.
            if (noOculusCredentials)
            {
                StartCoroutine(SpawnAvatar());
                return;
            }

            //Initialize the oculus platform
            try
            {
                Core.AsyncInitialize();
                Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
            }
            catch (UnityException e)
            {
                Debug.LogError("Platform failed to initialize due to exception.");
                Debug.LogException(e);
            }
        }

        void EntitlementCallback(Message msg)
        {
            if (msg.IsError)
            {
                Debug.LogError("You are NOT entitled to use this app. Please check if you added the correct ID's and credentials in Oculus>Platform");
                //UnityEngine.Application.Quit();
            }
            else
            {
                Debug.Log("You are entitled to use this app.");
                GetTokens();
            }
        }

        public override void OnJoinedRoom()
        {
            //Set the user to different spawning locations
            if (PhotonNetwork.LocalPlayer.ActorNumber <= spawnPoints.Length)
            {
                OVRCameraRig.transform.position = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.position;
                OVRCameraRig.transform.rotation = spawnPoints[PhotonNetwork.LocalPlayer.ActorNumber - 1].transform.rotation;
            }
            hasJoinedRoom = true;
        }

        //Get Access token and user ID from Oculus Platform
        private void GetTokens()
        {
            Users.GetAccessToken().OnComplete(message =>
            {
                if (!message.IsError)
                {
                    OvrAvatarEntitlement.SetAccessToken(message.Data);
                    Users.GetLoggedInUser().OnComplete(message =>
                    {
                        if (!message.IsError)
                        {
                            userID = message.Data.ID;
                            userIsEntitled = true;
                            StartCoroutine(SpawnAvatar());
                        }
                        else
                        {
                            var e = message.GetError();
                        }
                    });
                }
                else
                {
                    var e = message.GetError();
                }
            });
        }

        IEnumerator SpawnAvatar()
        {
            if (noOculusCredentials)
            {
                while (!hasJoinedRoom)
                {
                    yield return null;
                }
                myAvatarEntity = PhotonNetwork.Instantiate(avatarTestPrefab.name, OVRCameraRig.transform.position, OVRCameraRig.transform.rotation);
            }
            else
            {
                //Wait for all the entitlements and the runner to be ready to spawn
                while (!userIsEntitled || !OvrAvatarEntitlement.AccessTokenIsValid || !hasJoinedRoom)
                {
                    yield return null;
                }
                //Avatar spawning and seting its parent to be the OVR Camera Rig.
                myAvatarEntity = PhotonNetwork.Instantiate(avatarPrefab.name, OVRCameraRig.transform.position, OVRCameraRig.transform.rotation);
            }
            myAvatarEntity.transform.SetParent(OVRCameraRig.transform);
            myAvatarEntity.transform.localPosition = Vector3.zero;
            myAvatarEntity.transform.localRotation = Quaternion.identity;

            //Send an rpc with the Oculus UserID so other users can access to it and load our avatar.
            myAvatarEntity.GetComponent<PhotonView>().RPC("RPC_SetOculusID", RpcTarget.AllBuffered, (long)userID);

            //Instantiate the voice setup and set it as a child of the center eye anchor (head).
            var voiceSetup = PhotonNetwork.Instantiate(photonVoiceSetupPrefab.name, centerEyeAnchor.transform.position, centerEyeAnchor.transform.rotation);
            voiceSetup.transform.SetParent(centerEyeAnchor);
            voiceSetup.transform.localPosition = Vector3.zero;
            voiceSetup.transform.localRotation = Quaternion.identity;

            //Lipsync setup for our avatar
            myAvatarEntity.GetComponent<PunAvatarEntity>().SetLipSync(voiceSetup.GetComponent<OvrAvatarLipSyncContext>());
            voiceSetup.GetComponent<OvrAvatarLipSyncContext>().CaptureAudio = true;
        }

        public void ChangeMyAvatar(string assetPath)
        {
            if (myAvatarEntity == null) return;
            myAvatarEntity.GetComponent<PunAvatarEntity>().LoadNewAvatar(assetPath);
        }
    }
}
