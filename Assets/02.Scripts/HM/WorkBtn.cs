using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkBtn : MonoBehaviour
{
    public GameObject Keypad;
    public GameObject Pallete;
    public GameObject FileUI;
    public GameObject InfoUI;
    public GameObject VRkey;
    //public GameObject File_btn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void On()
    {
        Keypad.SetActive(true);
        Pallete.SetActive(false);

    }
    public void Off()
    {
        Keypad.SetActive(false);
        Pallete.SetActive(true);

    }
    public void File()
    {
        FileUI.SetActive(true);


    }
    public void FileClose()
    {
        FileUI.SetActive(false);


    }
    public void File_Info()
    {
        InfoUI.SetActive(true);
        VRkey.SetActive(true);

    }
    public void File_InfoDone()
    {
        InfoUI.SetActive(false);
        VRkey.SetActive(false);

    }
}
