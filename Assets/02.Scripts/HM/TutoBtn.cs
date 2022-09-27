using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutoBtn : MonoBehaviour
{
    public GameObject Keypad;
    public GameObject Pallete;
    public GameObject TutoUI;
    public GameObject Potal;
    public Image Info;
    public GameObject InfoUI;
    // Start is called before the first frame update
    void Start()
    {
        Keypad.SetActive(false);
        Pallete.SetActive(true);
        Potal.SetActive(false);
    }
    private void Update()
    {
        StartCoroutine(InfoCoroutine());
    }
    float time;
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
    public void TutoClose()
    {

        TutoUI.SetActive(false);
        Potal.SetActive(true);
    }
    
    IEnumerator InfoCoroutine()
    {
       //yield return new WaitForSeconds(2);
        for (float f = 1; f > 0; f -= 0.01f)
        {
            Color c = Info.GetComponent<Image>().color;
            c.a = f;
            Info.GetComponent<Image>().color = c;
            yield return null;
        }


        yield return new WaitForSeconds(2.0f);
        InfoUI.SetActive(false);

    }
}
