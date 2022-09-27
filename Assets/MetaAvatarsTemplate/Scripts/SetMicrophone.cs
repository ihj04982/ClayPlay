using Photon.Pun;
using UnityEngine;
using Photon.Voice.Unity;

namespace Chiligames.MetaAvatarsPun
{
    public class SetMicrophone : MonoBehaviourPun
    {
        //For making sure that microphone is found and set to "Recorder" component from Photon Voice
        private void Start()
        {
            if (photonView.IsMine)
            {
                string[] devices = Microphone.devices;
                if (devices.Length > 0)
                {
                    GetComponent<Recorder>().UnityMicrophoneDevice = devices[0];
                }
            }
        }
    }
}
