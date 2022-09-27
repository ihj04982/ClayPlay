using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExamplePosController : MonoBehaviour
{
    public Transform socketPos;
    float detectedRadius = 1;

    private void Start()
    {
        socketPos = GameObject.Find("Socket").transform;
    }
    void Update()
    {
        int layer = 1 << LayerMask.NameToLayer("Socket");
        Collider[] cols = Physics.OverlapSphere(transform.position, detectedRadius, layer);
        if (cols.Length > 0)
        { 
            Vector3 pos = new Vector3(socketPos.position.x, socketPos.position.y + socketPos.localScale.y + 0.1f + this.transform.localScale.y / 2, socketPos.position.z);
            this.transform.position = pos;
        }
    }
}
