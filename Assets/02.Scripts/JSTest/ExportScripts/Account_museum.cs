using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Web;
using System.Text;
using System.IO;
using System;
using TMPro;
using GLTFast;

public class Account_museum : MonoBehaviour
{
    public static Account_museum instance;
    public void Start()
    {
        this.mynick = PlayerPrefs.GetString("mynick");
        this.myid = PlayerPrefs.GetString("myid");
        StartCoroutine(GetMuseumWorksLenght("GetMuseumWorksLength"));
    }
    [SerializeField] string url = "https://w1hs1yfcgb.execute-api.ap-northeast-2.amazonaws.com/default/linktest";

    public void LoadingWorksFromMYSQL() => StartCoroutine(GetMuseumWorksLenght("GetMuseumWorksLength"));

    [SerializeField]
    public string mynick;
    [SerializeField]
    public string myid;

    int likecount = 0;
    public GameObject importlist;
    IEnumerator GetMuseumWorksLenght(string command)
    {
        WWWForm form = new WWWForm();
        form.AddField("command", command);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
        //DB에 등록된 작품 개수
        string worklist = www.downloadHandler.text;
        print(worklist);
        string[] split_text;
        split_text = worklist.Split(',');
/*
        for (int i = 0; i < split_text.Length; i++)
        {
            print(split_text[i]);
        }
*/
        print(split_text.Length);
        GltfAsset_Museum[] gltfassetlist = importlist.transform.GetComponentsInChildren<GltfAsset_Museum>(true);
        print("gltfastlengt" + gltfassetlist.Length);
        for (int i = 0; i < split_text.Length/7 ; i++)
        {
            if (i > gltfassetlist.Length-1)
            {
                break;
            }
            print(split_text[i * 7 + 3] + "efefdfdf");

            StartCoroutine(AccountDownloadUserData(split_text[i * 7 + 3]));
            print("jasondown");
            AWSManager.instance.LoadFromS3(split_text[i * 7 + 3] + ".bin");
            print("bindown");
            string path2 = Application.persistentDataPath + "/" + split_text[i*7 +3];
            print(path2);
            yield return new WaitForSeconds(0.5f);
            gltfassetlist[i].url = path2;
            gltfassetlist[i].gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = split_text[i*7 + 2];
            gltfassetlist[i].gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = split_text[i*7 + 6];
            gltfassetlist[i].gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = split_text[i*7 + 1];
            gltfassetlist[i].gameObject.transform.GetChild(0).transform.GetChild(0).transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = split_text[i*7 + 4];
            gltfassetlist[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            gltfassetlist[i].gameObject.transform.GetChild(4).localScale = new Vector3(0.5f, 0.5f, 0.5f);

        }
        //1. S3에서 bin 파일 요청 후 가져오기(1)
        //AWSManager.instance.LoadFromS3(opus + ".bin");
        //2. jasonfile 가져오기
        //StartCoroutine(AccountDownloadUserData(opus));

    }

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
    }
  
    
}