using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;

    public TMP_InputField nameInput;
    public bool isMyRoom = false;
    public bool isMuseum = false;
    public bool isTuto = false;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.AutomaticallySyncScene = true;
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Try Connect To Server");
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server");
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }


    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("OnJoinedLobby");
        if (isMyRoom == true && isTuto == false )
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 100;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;
            PhotonNetwork.JoinOrCreateRoom("Museum", roomOptions, TypedLobby.Default);
            PhotonNetwork.LoadLevel(2);
        }
        else if (isMuseum)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 1;
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = false;
            PhotonNetwork.JoinOrCreateRoom(PhotonNetwork.LocalPlayer.NickName, roomOptions, TypedLobby.Default);
            PhotonNetwork.LoadLevel(1);
        }
        else if (isTuto)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 1;
            roomOptions.IsVisible = false;
            roomOptions.IsOpen = false;
            PhotonNetwork.JoinOrCreateRoom(PhotonNetwork.LocalPlayer.NickName, roomOptions, TypedLobby.Default);
            PhotonNetwork.LoadLevel(1);
            isTuto = false;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log($"현재 방 {PhotonNetwork.CurrentRoom.Name} 접속");
        if (PhotonNetwork.CurrentRoom.Name == PhotonNetwork.LocalPlayer.NickName && isTuto != true)
        {
            isMyRoom = true;
            isMuseum = false;
            print(isMyRoom);
            print(isTuto);
        }
        if (PhotonNetwork.CurrentRoom.Name == "Museum")
        {
            isMuseum = true;
            isMyRoom = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"플레이어 {newPlayer.NickName} 방 참가.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"플레이어 {otherPlayer.NickName} 방 나감.");
    }

    public void Connect_Work()
    {
        if (nameInput.text != "")
        {
            PhotonNetwork.LocalPlayer.NickName = nameInput.text;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "User" + Random.Range(1, 9999);
            nameInput.text = PhotonNetwork.LocalPlayer.NickName;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = false;
        PhotonNetwork.JoinOrCreateRoom(PhotonNetwork.LocalPlayer.NickName, roomOptions, TypedLobby.Default);

    }

    public void ConnectToMuseum()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void MuseumToRoom()
    {
        PhotonNetwork.LeaveRoom();
    }    
    
    public void TutoToRoom()
    {
        PhotonNetwork.LeaveRoom();
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 1;
        roomOptions.IsVisible = false;
        roomOptions.IsOpen = false;
        PhotonNetwork.JoinOrCreateRoom(PhotonNetwork.LocalPlayer.NickName, roomOptions, TypedLobby.Default);
    }
    public void OnSceneButtonClick(int index)
    {
        PhotonNetwork.LoadLevel(index);
    }
    public void OnTutoClick(bool value)
    {
            isTuto = value;
    }

}