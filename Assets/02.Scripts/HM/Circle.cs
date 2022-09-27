using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public GameObject Load;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Loadig());
    }
    IEnumerator Loadig()
    {


        yield return new WaitForSeconds(5);
        Load.SetActive(false);

    }
}
