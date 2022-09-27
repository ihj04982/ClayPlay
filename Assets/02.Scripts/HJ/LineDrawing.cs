using System.Collections.Generic;

using UnityEngine;

public class LineDrawing : MonoBehaviour
{


    [Header("Line Components")]
    public bool isStart = false;
    public bool isDrawing = false;
    public bool isConnect = false;
    public bool isUIClick = true;
    public float drawDist = 0.1f;
    public GameObject line;
    public Transform target;
    public Transform linePool;
    public List<LineRenderer> lineList = new List<LineRenderer>();
    LineRenderer curLine;
    private int positionCount = 2;
    private Vector3 PrevPos = Vector3.zero;
    private Vector3 PrevPipePos = Vector3.zero;
    public Color color;


    [Header("Pipe Components")]
    public GameObject pipe;
    public Transform pipePool;
    public List<GameObject> pipeList = new List<GameObject>();
    PipeGenerator pipeGenerator;

    public void OnUIClick(bool value)
    {
        if (value == true)
            isUIClick = true;
        else
            isUIClick = false;
    }
    public void OnDrawingButtonClick(bool value)
    {
        if (value == true)
            isStart = true;
        else
            isStart = false;
    }
    public void DrawingStart()
    {
        isDrawing = true;
        isConnect = true;

    }
    public void DrawingEnd()
    {
        if (isStart)
        {
            print("gmss");
            isDrawing = false;
            isConnect = false;
            if (pipeList.Count > 0)
            {
                GeneratePipe();
            }
            if (lineList.Count > 0)
            {
                foreach (LineRenderer item in lineList)
                {
                    Destroy(item.gameObject);
                }
                lineList.Clear();
            }
        }
    }
    void Update()
    {
        if (isStart)
        {
            if (isUIClick == false)
            {
                if (isConnect == true)
                {
                    Vector3 mousePos = target.position;
                    if (isDrawing == true)
                    {
                        createLine(mousePos);
                    }
                    connectLine(mousePos);
                }
            }
        }
    }




    void createLine(Vector3 mousePos)
    {
        GameObject lineInstance = Instantiate(line);
        curLine = lineInstance.GetComponent<LineRenderer>();
        color = ColorPicker.instance.color;
        curLine.startColor = color;
        curLine.endColor = color;
        positionCount = 2;
        curLine.transform.position = mousePos;
        curLine.transform.SetParent(linePool);
        lineList.Add(curLine);
        curLine.SetPosition(0, mousePos);
        curLine.SetPosition(1, mousePos);

        GameObject pipeInstance = Instantiate(pipe);
        
        pipeInstance.transform.SetParent(pipePool);
        pipeGenerator = pipeInstance.GetComponent<PipeGenerator>();

        isDrawing = false;
    }
    void connectLine(Vector3 mousePos)
    {

        if (PrevPos != null && Mathf.Abs(Vector3.Distance(PrevPos, mousePos)) >= drawDist)
        {
            PrevPos = mousePos;
            positionCount++;
            curLine.positionCount = positionCount;
            curLine.SetPosition(positionCount - 1, mousePos);
        }
        if (PrevPipePos != null && Mathf.Abs(Vector3.Distance(PrevPipePos, mousePos)) >= 0.08f)
        {
            PrevPipePos = mousePos;
            pipeGenerator.points.Add(PrevPos / 0.3f);
        }
    }
    public void GeneratePipe()
    {
        GameObject curPipe = pipeList[pipeList.Count - 1];
        curPipe.GetComponent<PipeGenerator>().RenderPipe();
        curPipe.GetComponent<Renderer>().material.color = ColorPicker.instance.color;
    }
    public void Undo()
    {
        if (pipeList.Count > 0)
        {
            GameObject undo = pipeList[pipeList.Count - 1];
            Destroy(undo);
            pipeList.RemoveAt(pipeList.Count - 1);
        }
    }
    public void ClearScreen()
    {
        foreach (GameObject item in pipeList)
        {
            Destroy(item);
        }
        pipeList.Clear();
    }
}
