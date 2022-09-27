using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Random = UnityEngine.Random;
using GLTFast;
public class LoginRegManager : MonoBehaviour
{
    public static LoginRegManager instance;
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    [SerializeField] string url = "https://w1hs1yfcgb.execute-api.ap-northeast-2.amazonaws.com/default/linktest";
    // Start is called before the first frame update
    public void OnLogin() => StartCoroutine(IELogin("login"));
    public void OnRegister() => StartCoroutine(IERegister("register"));

    [SerializeField]
    public string mynick;
    [SerializeField]
    public string myid = "_";
    [SerializeField] TMP_InputField loginidInput;
    [SerializeField] TMP_InputField loginpasswordInput;

    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_InputField nicknameInput;

    IEnumerator IELogin(string command)
    {
        WWWForm form = new WWWForm();
        
        form.AddField("command", command);
        // 주석 풀고 inputfield 연결
        form.AddField("id", loginidInput.text);
        form.AddField("password", loginpasswordInput.text);
        PlayerPrefs.SetString("myid", loginidInput.text);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        print(www.downloadHandler.text);

        //mynick 아래 지워도 됌
        mynick = www.downloadHandler.text;
        PlayerPrefs.SetString("mynick", www.downloadHandler.text);
        
    }
    IEnumerator IERegister(string command)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", command);

        // 주석 풀고 inputfield 연결
              
        form.AddField("id", idInput.text);
        form.AddField("password", passwordInput.text);
        form.AddField("nickname", nicknameInput.text);
        PlayerPrefs.SetString("myid", idInput.text);
        PlayerPrefs.SetString("mynick", nicknameInput.text);
     // -------------아래로 필요 없을 수도------------
        myid = idInput.text;
        mynick = nicknameInput.text;
        
/*
        // ------------tmp 테스트 후 지우기 -----------
        string idtmp = "id" + Random.Range(0, 10000).ToString("00000");
        string passwordtmp = "pass" + Random.Range(0, 10000).ToString("00000");
        string nicktmp = "nick" + Random.Range(0, 10000).ToString("00000");
        form.AddField("id", idtmp);
        form.AddField("password", passwordtmp);
        form.AddField("nickname", nicktmp);
        PlayerPrefs.SetString("myid", idtmp);
        PlayerPrefs.SetString("mynick", nicktmp);
        // -------------------------------------------
*/
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        PlayerPrefs.SetString("Failtoregister", www.downloadHandler.text);
        print(www.downloadHandler.text);
    }
}
