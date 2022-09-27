using UnityEngine;
using GLTFast.Export;
public class TestExport : MonoBehaviour
{
    private void Start()
    {

    }


    public async void SimpleExport()
    {
        string opus = "OBJ_" + Random.Range(1, 10000).ToString("00000");
        PlayerPrefs.SetString("Opus", opus);
        //Application.persistentDataPath + "/" + name;
        //string path = "C:/Final/ServerTest/Assets/ExportObj/" + opus;
        string path = Application.persistentDataPath + "/" + opus;
        // Example of gathering GameObjects to be exported (recursively)
        var rootLevelNodes = GameObject.FindGameObjectWithTag("ExportMe");
        GameObject[] tmpObj = null;
        if (rootLevelNodes.transform.childCount != 0)
        {
            tmpObj = new GameObject[rootLevelNodes.transform.childCount];
            for (int i = 0; i < rootLevelNodes.transform.childCount; i++)
            {
                tmpObj[i] = rootLevelNodes.transform.GetChild(i).gameObject;
                tmpObj[i].transform.localPosition = Vector3.zero;
            }
        }
        // GameObjectExport lets you create glTFs from GameObject hierarchies
        var export = new GameObjectExport();

        // Add a scene
        export.AddScene(tmpObj);
        
        // Async glTF export
        bool success = await export.SaveToFileAndDispose(path);
        //bool success2 = await export.SaveToStreamAndDispose(stream);
        
        if (!success)
        {
            Debug.LogError("Something went wrong exporting a glTF");
        }
        else
        {
            Debug.Log("Success exporting a glTF");
        }
    }
    //일어나세요 용사여 ...

}