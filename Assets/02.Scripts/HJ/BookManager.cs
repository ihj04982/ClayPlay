using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public GameObject[] Prefabs;
    public List<GameObject> examples = new List<GameObject>();
    public Transform targetPos;

    // Start is called before the first frame update
    private void Update()
    {
        //examples = GameObject.FindGameObjectsWithTag("Example");
    }
    public void GetImangeIndex(int idx)
    {
        Instantiater(idx);
    }


    public void Instantiater(int idx)
    {

        if (examples.Count != 0)
        {
            Destroy(examples[0].gameObject);
            examples.Clear();            
        }
        Vector3 pos = new Vector3(targetPos.position.x, targetPos.position.y + targetPos.localScale.y / 2 + Prefabs[idx].transform.localScale.y / 2, targetPos.position.z);
        GameObject go = Instantiate(Prefabs[idx], pos, transform.rotation);
        examples.Insert(0, go);
    }
  

}
