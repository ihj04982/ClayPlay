using UnityEngine;

public class BookOpen : MonoBehaviour
{
    public GameObject Props;

    // Start is called before the first frame update
    void Start()
    {
        Props.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {
            if (Props.activeSelf == true)
            {
                Props.SetActive(false);
                Debug.Log("open");
            }
            else
            {
                Props.SetActive(true);
                Debug.Log("close");
            }
        }
    }
}
