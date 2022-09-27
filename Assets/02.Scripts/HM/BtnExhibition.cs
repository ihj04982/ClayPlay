using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnExhibition : MonoBehaviour
{
    public GameObject Explane;
    public void On()
    {
        Explane.SetActive(true);
        

    }
    public void Off()
    {
        Explane.SetActive(false);
     

    }
}
