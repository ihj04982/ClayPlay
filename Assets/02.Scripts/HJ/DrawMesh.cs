using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMesh : MonoBehaviour
{
    private Camera cam;
    public Transform targetPos;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            StartCoroutine("Draw");
        }
        else
            EndDraw();
        if (Input.GetMouseButton(1))
        {
            Debug.Log(Input.mousePosition);
        }

    }
    public void StartDraw()
    {
        StartCoroutine("Draw");


    }
    private IEnumerator Draw()
    {
        GameObject drawing = new GameObject("Drawing");
        drawing.AddComponent<MeshFilter>();
        drawing.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>(new Vector3[8]);
        // Vector3 startPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
        Vector3 startPosition = targetPos.position;
        Vector3 temp = new Vector3(startPosition.x, startPosition.y, 0.5f);
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = temp;
        }
        List<int> triangles = new List<int>(new int[36]);
        // Front
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 0;
        triangles[4] = 3;
        triangles[5] = 2;

        // Top
        triangles[6] = 2;
        triangles[7] = 2;
        triangles[8] = 4;
        triangles[9] = 2;
        triangles[10] = 4;
        triangles[11] = 5;

        // Top
        triangles[12] = 1;
        triangles[13] = 2;
        triangles[14] = 5;
        triangles[15] = 1;
        triangles[16] = 5;
        triangles[17] = 6;

        // Left
        triangles[18] = 0;
        triangles[19] = 7;
        triangles[20] = 4;
        triangles[21] = 0;
        triangles[22] = 4;
        triangles[23] = 3;

        // Back
        triangles[24] = 5;
        triangles[25] = 4;
        triangles[26] = 7;
        triangles[27] = 5;
        triangles[28] = 7;
        triangles[29] = 6;

        // Bottom
        triangles[30] = 0;
        triangles[31] = 6;
        triangles[32] = 7;
        triangles[33] = 0;
        triangles[34] = 1;
        triangles[35] = 6;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        drawing.GetComponent<MeshFilter>().mesh = mesh;
        drawing.GetComponent<MeshRenderer>().material.color = Color.blue;

        Vector3 lastTargetPos = startPosition;

        while (true)
        {

            float minDistance = 0.1f;
            float distance = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), lastTargetPos);
            while (distance < minDistance)
            {
                distance = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), lastTargetPos);
                yield return null;
            }
            vertices.AddRange(new Vector3[4]);
            triangles.AddRange(new int[30]);

            int vIndex = vertices.Count - 8;
            int vIndex0 = vIndex + 3;
            int vIndex1 = vIndex + 2;
            int vIndex2 = vIndex + 1;
            int vIndex3 = vIndex + 0;
            int vIndex4 = vIndex + 4;
            int vIndex5 = vIndex + 5;
            int vIndex6 = vIndex + 6;
            int vIndex7 = vIndex + 7;

            //Vector3 currentTargePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            Vector3 currentTargePos = targetPos.position;
            Vector3 mouseforwardVector = (currentTargePos - lastTargetPos).normalized;

            float lineTickness = 0.25f;

            Vector3 topRightVertex = currentTargePos + Vector3.Cross(mouseforwardVector, Vector3.back) * lineTickness;
            Vector3 bottomRightVertex = currentTargePos + Vector3.Cross(mouseforwardVector, Vector3.forward) * lineTickness;
            Vector3 topLeftVertex = new Vector3(topRightVertex.x, topRightVertex.y, 1);
            Vector3 bottomLeftVertex = new Vector3(bottomRightVertex.x, bottomRightVertex.y, 1);

            vertices[vIndex4] = topLeftVertex;
            vertices[vIndex5] = topRightVertex;
            vertices[vIndex6] = bottomRightVertex;
            vertices[vIndex7] = bottomLeftVertex;

            int tIndex = triangles.Count - 30;

            triangles[tIndex + 0] = vIndex2;
            triangles[tIndex + 1] = vIndex3;
            triangles[tIndex + 2] = vIndex4;
            triangles[tIndex + 3] = vIndex2;
            triangles[tIndex + 4] = vIndex4;
            triangles[tIndex + 5] = vIndex5;

            triangles[tIndex + 6] = vIndex1;
            triangles[tIndex + 7] = vIndex2;
            triangles[tIndex + 8] = vIndex5;
            triangles[tIndex + 9] = vIndex1;
            triangles[tIndex + 10] = vIndex5;
            triangles[tIndex + 11] = vIndex6;

            triangles[tIndex + 12] = vIndex0;
            triangles[tIndex + 13] = vIndex7;
            triangles[tIndex + 14] = vIndex4;
            triangles[tIndex + 15] = vIndex0;
            triangles[tIndex + 16] = vIndex4;
            triangles[tIndex + 17] = vIndex3;

            triangles[tIndex + 18] = vIndex5;
            triangles[tIndex + 19] = vIndex4;
            triangles[tIndex + 20] = vIndex7;
            triangles[tIndex + 21] = vIndex0;
            triangles[tIndex + 22] = vIndex4;
            triangles[tIndex + 23] = vIndex3;

            triangles[tIndex + 24] = vIndex0;
            triangles[tIndex + 25] = vIndex6;
            triangles[tIndex + 26] = vIndex7;
            triangles[tIndex + 27] = vIndex0;
            triangles[tIndex + 28] = vIndex1;
            triangles[tIndex + 29] = vIndex6;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            // lastTargetPos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            lastTargetPos = targetPos.position;
            mesh.RecalculateNormals();
            yield return null;
        }
    }
    public void EndDraw()
    {

    }

}
