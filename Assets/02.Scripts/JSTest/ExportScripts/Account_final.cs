using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Text;
using System.IO;
using System;
using TMPro;
using GLTFast;

public class Account_final : MonoBehaviour
{
    public static Account_final instance;
    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        mynick = LoginRegManager.instance.mynick;
        myid = LoginRegManager.instance.myid;
    }

    [SerializeField] string url = "https://w1hs1yfcgb.execute-api.ap-northeast-2.amazonaws.com/default/linktest";


    public void OnLogin() => StartCoroutine(IELogin("login"));
    public void OnRegister() => StartCoroutine(IERegister("register"));

  // public void OnJsonFileUpload() => StartCoroutine(AccountUploadJsonFile("UploadObjectJson")); // 전시실 방에 연결해야하는 버튼

    // 테스트 한 DB 매서드 버튼
    public void ONUploadMYSQL() => StartCoroutine(AccountUploadMYSQL("UploadObj")); // mysql에서 jsonFile 업로드

    // 전시실 불러오기 버튼 => local에서 / S3에서 불러오기
    public void OnSceneLoadFromS3() => StartCoroutine(("downLoadDB")); // mysql에서 jsonFile 다운로드

    public void GetFileNameFromMYSQL() => StartCoroutine(GetfileName("GetFileName"));

    [SerializeField]
    public string mynick;
    [SerializeField]
    public string myid ="_";
    [SerializeField] TMP_InputField idInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_InputField nicknameInput;
    [SerializeField] TMP_InputField titleInput;
    [SerializeField] TMP_InputField InfoInput;
    IEnumerator IELogin(string command)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", command);
        form.AddField("id", idInput.text);
        form.AddField("password", passwordInput.text);
        myid = idInput.text;
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        print(www.downloadHandler.text);
        mynick = www.downloadHandler.text;
    }
    IEnumerator IERegister(string command)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", command);
        form.AddField("id", idInput.text);
        form.AddField("password", passwordInput.text);
        form.AddField("nickname", nicknameInput.text);
        myid = idInput.text;
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        print(www.downloadHandler.text);
    }
    /*
        IEnumerator AccountUploadJsonFile(string command)
        {
            WWWForm form = new WWWForm();
            form.AddField("command", command);
           // form.AddField("id", Account.instance.myid);

            // string tmp = SceneSaveManager.instance.DbUpLoad();
            // Debug.Log(tmp);
            // form.AddField("jsonFile", tmp);

            // 파일명을 랜덤하게 설정해서 전달
            string opus = PlayerPrefs.GetString("Opus");
            form.AddField("jsonFileName", opus);

            // Debug.Log(jsonFileName);

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            yield return www.SendWebRequest();
            print(www.downloadHandler.text);
        }
    */
    IEnumerator GetfileName(string command)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", command);
        //form.AddField("id", "0904SHL7");
        form.AddField("id", myid);

        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();
        string opus = www.downloadHandler.text;
        print(opus);
        //1. S3에서 bin 파일 요청 후 가져오기(1)
        AWSManager.instance.LoadFromS3(opus + ".bin");

        //2. jasonfile 가져오기
        StartCoroutine(AccountDownloadUserData(opus));

    }
    public GameObject importlist;
    IEnumerator AccountDownloadUserData(string opus)
    {
        WWWForm form = new WWWForm();
        // form.AddField("jsonFile", tmp);

        // // 파일명을 랜덤하게 설정해서 전달
        // string jsonFileName = Account.instance.myid + "_" + Random.Range(1, 10000).ToString("00000");
        // form.AddField("jsonFileName", jsonFileName);
        //Debug.Log(jsonFileName);
        form.AddField("command", "downLoadDB");
        form.AddField("id", myid);
        //form.AddField("id", "0904SHL7");
        form.AddField("jsonFileName", opus);
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        string originalJson = HttpUtility.UrlDecode(www.downloadHandler.text, Encoding.UTF8);
        print(originalJson);
        string path2 = "C:/Final/ServerTest/Assets/ExportObj/" + opus;
        //string path2 = "C:/Project/FinalProject_LinkinArt_Final_SHL/Assets/jsonResults/" + jsonFileName;
        File.WriteAllText(path2, originalJson);
        Debug.Log(path2);
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < importlist.transform.childCount; i++)
        {
            importlist.transform.GetChild(i).GetComponent<GltfAsset>().url = path2;
            importlist.transform.GetChild(i).gameObject.SetActive(true);
        }
       // json = www.downloadHandler.text;
       //print(originalJson);
       //  SceneSerialization.ImportScene(originalJson);
       // Debug.Log(originalJson);

       // print(json);
       //  print(www.downloadHandler.text);

       PlayerPrefs.SetString("S3_FILE_NAME", opus);
    }
    int likecount =0;
    //DB 테스트용 매서드
    IEnumerator AccountUploadMYSQL(string command)
    {
        yield return new WaitForSeconds(2f);
        WWWForm form = new WWWForm();
        form.AddField("command", command);

        //form.AddField("nickName", nickNameInput.text);
        string fileopus = PlayerPrefs.GetString("Opus");
        string path = "C:/Final/ServerTest/Assets/ExportObj/" + fileopus;

        print(fileopus);
        FileStream fs = new FileStream(path, FileMode.Open);
        byte[] data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        fs.Close();
        string jason = Encoding.UTF8.GetString(data);
        form.AddField("id", myid);
        form.AddField("jsonFile", jason);
        form.AddField("jsonFileopus", fileopus);
        form.AddField("nickname", mynick);
        form.AddField("title", titleInput.text);
        form.AddField("info", InfoInput.text);
        form.AddField("date", DateTime.Now.ToString("d"));
        form.AddField("likecount", likecount.ToString());
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();
        print(www.downloadHandler.text);
        AWSManager.instance.UploadToS3(path +".bin", fileopus + ".bin");
        //PlayerPrefs.SetString("S3_FILE_NAME", jsonFileName);

    }

}