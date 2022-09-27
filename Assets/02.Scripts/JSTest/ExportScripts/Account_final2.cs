using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Text;
using System.IO;
using System;
using TMPro;
using GLTFast;

public class Account_final2 : MonoBehaviour
{
    public static Account_final2 instance;
    public void Start()
    {
        this.mynick = PlayerPrefs.GetString("mynick");
        this.myid = PlayerPrefs.GetString("myid");
    }
    [SerializeField] string url = "https://w1hs1yfcgb.execute-api.ap-northeast-2.amazonaws.com/default/linktest";

  // public void OnJsonFileUpload() => StartCoroutine(AccountUploadJsonFile("UploadObjectJson")); // ���ý� �濡 �����ؾ��ϴ� ��ư

    // �׽�Ʈ �� DB �ż��� ��ư
    public void ONUploadMYSQL() => StartCoroutine(AccountUploadMYSQL("UploadObj")); // mysql���� jsonFile ���ε�

    public void ONUploadToMuseumMYSQL() => StartCoroutine(AccountUploadMYSQL("UploadObjToMuseumTBL"));
    // ���ý� �ҷ����� ��ư => local���� / S3���� �ҷ�����
    public void OnSceneLoadFromS3() => StartCoroutine(("downLoadDB")); // mysql���� jsonFile �ٿ�ε�

    public void GetFileNameFromMYSQL() => StartCoroutine(GetfileName("GetFileName"));

    [SerializeField]
    public string mynick;
    [SerializeField]
    public string myid ="_";
    [SerializeField] 
    public TMP_InputField titleInput;
    [SerializeField] 
    public TMP_InputField InfoInput;
    public GameObject importer;
    public GameObject claymanager;
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
        //1. S3���� bin ���� ��û �� ��������(1)
        AWSManager.instance.LoadFromS3(opus + ".bin");
        //2. jasonfile ��������
        StartCoroutine(AccountDownloadUserData(opus));

    }
    public GameObject importlist;
    IEnumerator AccountDownloadUserData(string opus)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", "downLoadDB");
        form.AddField("id", myid);
        form.AddField("jsonFileName", opus);
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();

        string originalJson = HttpUtility.UrlDecode(www.downloadHandler.text, Encoding.UTF8);
        print(originalJson);
        string path2 = Application.persistentDataPath + "/" + opus;
        File.WriteAllText(path2, originalJson);


        Debug.Log(path2);
        yield return new WaitForSeconds(1f);
        Transform importtmp = importlist.transform.GetChild(0);
        importtmp.GetComponent<GltfAsset>().url = path2;

        importtmp.GetComponent<GltfAsset>().ImportOBJ();
        yield return new WaitForSeconds(1f);
        //����Ʈ�� �۾��� claymanager ������ �ֱ�
        if (importtmp.gameObject.transform.childCount != 0)
        {
            for (int i = 0; i < importtmp.gameObject.transform.childCount; i++) { 
            importtmp.gameObject.transform.GetChild(i).SetParent(claymanager.transform);
                }
        }
        yield return new WaitForSeconds(0.5f);

       PlayerPrefs.SetString("S3_FILE_NAME", opus);
    }
    int likecount =0;
    //DB �׽�Ʈ�� �ż���
    IEnumerator AccountUploadMYSQL(string command)
    {
        yield return new WaitForSeconds(2f);
        WWWForm form = new WWWForm();
        form.AddField("command", command);

        string fileopus = PlayerPrefs.GetString("Opus");
        //string path = "C:/Final/ServerTest/Assets/ExportObj/" + fileopus;
        string path = Application.persistentDataPath + "/" + fileopus;

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

        //title input�ʵ�� ����
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