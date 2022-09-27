using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOpen : MonoBehaviour
{
    public GameObject Keypad;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     public void OpenMenu()
    {
        Keypad.SetActive(true);
    }
}
