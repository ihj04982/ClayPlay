using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class NetworkManager1 : MonoBehaviourPunCallbacks
{
    public void OnNextSceneButtonClick(int index) 
    {
        PhotonNetwork.LoadLevel(index);
    }
}