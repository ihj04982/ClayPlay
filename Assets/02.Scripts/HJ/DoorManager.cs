using UnityEngine;

public class DoorManager : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(NetworkManager.instance.isMyRoom)
            {
            NetworkManager.instance.ConnectToMuseum();
            }
            if (NetworkManager.instance.isMuseum)
            {
                NetworkManager.instance.MuseumToRoom();
            }
            else
            {
                NetworkManager.instance.TutoToRoom();
            }

        }
    }
}

