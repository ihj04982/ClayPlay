using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager_JOY : MonoBehaviour
{
    public GameObject ex_Prefab1;
    public GameObject ex_Prefab2;
    public Transform targetPos;
    // Start is called before the first frame update

    public void Instantiater1()
    {
        Vector3 pos = new Vector3(targetPos.position.x, targetPos.position.y + targetPos.localScale.y / 2 + ex_Prefab1.transform.localScale.y / 2, targetPos.position.z);
        GameObject prefab = GameObject.Instantiate(ex_Prefab1, pos, transform.rotation);
        //prefab.transform.SetParent(targetPos);
    }
    public void Instantiater2()
    {
        Vector3 pos = new Vector3(targetPos.position.x, targetPos.position.y + targetPos.localScale.y / 2 + ex_Prefab2.transform.localScale.y / 2, targetPos.position.z);
        GameObject prefab = GameObject.Instantiate(ex_Prefab2, pos, transform.rotation);
        //prefab.transform.SetParent(targetPos);
    }
}
