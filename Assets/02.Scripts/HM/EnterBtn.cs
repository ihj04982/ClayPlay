using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterBtn : MonoBehaviour
{

    public GameObject LoginUI;
    public GameObject Keyboard;
    public GameObject AskUI;
    public GameObject LogoUI;
    public Image Logo;
    public Image LoginImg;
    public TMP_Text[] texts;
    public GameObject RegisterUI;
    public GameObject LoginFail;
    public GameObject RegisterFail;

    public void OnLogin() => StartCoroutine(DelayLogin());
    public void OnResister() => StartCoroutine(DelayRegister());
    IEnumerator DelayLogin()
    {
        yield return new WaitForSeconds(1f);
        Login();
    }

    IEnumerator DelayRegister()
    {
        yield return new WaitForSeconds(1f);
        Ok();
    }
    public void Login()
    {
        if (string.Compare(PlayerPrefs.GetString("mynick"), "loginFail") == 0)
        {
            // Login 실패 무언가 표시
            LoginFail.SetActive(true);
            
            StartCoroutine(Login_Faild());

        }
        else if ((string.Compare(PlayerPrefs.GetString("mynick"), "loginFail") != 0))
        {
            StartCoroutine(FadeLoginCoroutine());
        }
    }
    IEnumerator Login_Faild()
    {
    

        yield return new WaitForSeconds(2);
        LoginFail.SetActive(false);

    }
    IEnumerator FadeLoginCoroutine()
    {
        for (float f = 1f; f > 0; f -= 0.02f)
        {

            Color cn = LoginImg.GetComponent<Image>().color;
            cn.a = f;
            LoginImg.GetComponent<Image>().color = cn;
            texts[0].color = cn;
            texts[1].color = cn;
            yield return null;

        }

        yield return new WaitForSeconds(1);
        LoginUI.SetActive(false);
        Keyboard.SetActive(false);
        AskUI.SetActive(true);

    }
    public void Go()
    {
        StartCoroutine(FadeLogoCoroutine());
    }
    IEnumerator FadeLogoCoroutine()
    {

        for (float f = 1f; f > 0; f -= 0.02f)
        {
            Color c = Logo.GetComponent<Image>().color;
            c.a = f;
            Logo.GetComponent<Image>().color = c;
            yield return null;
        }
        yield return new WaitForSeconds(1);
        //AskUI.SetActive(true);
        LogoUI.SetActive(false);
        LoginUI.SetActive(true);
        Keyboard.SetActive(true);
        LoginFail.SetActive(false);

    }
    public void Yes()
    {

        SceneManager.LoadScene("Tuto");
    }
    public void No()
    {

        SceneManager.LoadScene("Work_JOY");
    }
    public void Register()
    {
        LoginUI.SetActive(false);
        RegisterUI.SetActive(true);
        RegisterFail.SetActive(false);
    }
    public void Ok()
    {
        //Resister 실패 시
        if (string.Compare(PlayerPrefs.GetString("Failtoregister"), "Failtoregister") == 0)
        {
            RegisterFail.SetActive(true);
            StartCoroutine(Register_Fail());
        }


        else if (string.Compare(PlayerPrefs.GetString("Failtoregister"), "Failtoregister") != 0)
        {
            LoginUI.SetActive(true);
            RegisterUI.SetActive(false);
        }
        
    }
    IEnumerator Register_Fail()
    {

        yield return new WaitForSeconds(2);
        RegisterFail.SetActive(false);

    }
}