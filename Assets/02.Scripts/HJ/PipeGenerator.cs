using System.Collections.Generic;
using UnityEngine;

public class PipeGenerator : MonoBehaviour
{

    public List<Vector3> points;
    float pipeRadius = 0.08f;
    float elbowRadius = 0.1f;
    [Range(3, 32)]
    public int pipeSegments = 8;
    [Range(3, 32)]
    public int elbowSegments = 6;
    public float colinearThreshold = 0.1f;
    public Material pipeMaterial;
    bool avoidStrangling = true;
    bool generateEndCaps = true;
    bool generateElbows = true;
    MeshDeformer meshDeformer;

    private void Start()
    {
        meshDeformer = this.GetComponent<MeshDeformer>();
    }

    public void RenderPipe()
    {
        if (points.Count > 2)
        {
            MeshFilter currentMeshFilter = GetComponent<MeshFilter>();
            MeshFilter mf = currentMeshFilter != null ? currentMeshFilter : gameObject.AddComponent<MeshFilter>();
            Mesh mesh = GenerateMesh();
            mf.mesh = mesh;

            MeshRenderer currentMeshRenderer = GetComponent<MeshRenderer>();
            MeshRenderer mr = currentMeshRenderer != null ? currentMeshRenderer : gameObject.AddComponent<MeshRenderer>();
            mr.materials = new Material[1] { pipeMaterial };

            meshDeformer.GetMesh();
        }

    }
    Mesh GenerateMesh()
    {
        Mesh m = new Mesh();
        m.name = "UnityPlumber Pipe";
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // for each segment, generate a cylinder
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 initialPoint = points[i];
            Vector3 endPoint = points[i + 1];
            Vector3 direction = (points[i + 1] - points[i]).normalized;

            if (i > 0 && generateElbows)
            {
                initialPoint = initialPoint + direction * elbowRadius;
            }

            if (i < points.Count - 1 && generateElbows)
            {
                endPoint = endPoint - direction * elbowRadius;
            }

            GenerateCircleAtPoint(vertices, normals, initialPoint, direction);
            GenerateCircleAtPoint(vertices, normals, endPoint, direction);
            MakeCylinderTriangles(triangles, i);
        }

        // for each segment generate the elbow that connects it to the next one
        if (generateElbows)
        {
            for (int i = 0; i < points.Count - 2; i++)
            {
                Vector3 point1 = points[i]; // starting point
                Vector3 point2 = points[i + 1]; // the point around which the elbow will be built
                Vector3 point3 = points[i + 2]; // next point
                GenerateElbow(i, vertices, normals, triangles, point1, point2, point3);
            }
        }

        if (generateEndCaps)
        {
            GenerateEndCaps(vertices, triangles, normals);
        }

        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetNormals(normals);
        return m;

    }

    void GenerateElbow(int index, List<Vector3> vertices, List<Vector3> normals, List<int> triangles, Vector3 point1, Vector3 point2, Vector3 point3)
    {
        // generates the elbow around the area of point2, connecting the cylinders
        // corresponding to the segments point1-point2 and point2-point3
        Vector3 offset1 = (point2 - point1).normalized * elbowRadius;
        Vector3 offset2 = (point3 - point2).normalized * elbowRadius;
        Vector3 startPoint = point2 - offset1;
        Vector3 endPoint = point2 + offset2;
        // auxiliary vectors to calculate lines parallel to the edge of each
        // cylinder, so the point where they meet can be the center of the elbow
        Vector3 perpendicularToBoth = Vector3.Cross(offset1, offset2);
        Vector3 startDir = Vector3.Cross(perpendicularToBoth, offset1).normalized;
        Vector3 endDir = Vector3.Cross(perpendicularToBoth, offset2).normalized;

        // calculate torus arc center as the place where two lines projecting
        // from the edges of each cylinder intersect
        Vector3 torusCenter1;
        Vector3 torusCenter2;
        Math3D.ClosestPointsOnTwoLines(out torusCenter1, out torusCenter2, startPoint, startDir, endPoint, endDir);
        Vector3 torusCenter = 0.5f * (torusCenter1 + torusCenter2);

        // calculate actual torus radius based on the calculated center of the 
        // torus and the point where the arc starts
        float actualTorusRadius = (torusCenter - startPoint).magnitude;

        float angle = Vector3.Angle(startPoint - torusCenter, endPoint - torusCenter);
        float radiansPerSegment = (angle * Mathf.Deg2Rad) / elbowSegments;
        Vector3 lastPoint = point2 - startPoint;

        for (int i = 0; i <= elbowSegments; i++)
        {
            // create a coordinate system to build the circular arc
            // for the torus segments center positions
            Vector3 xAxis = (startPoint - torusCenter).normalized;
            Vector3 yAxis = (endPoint - torusCenter).normalized;
            Vector3.OrthoNormalize(ref xAxis, ref yAxis);

            Vector3 circleCenter = torusCenter +
                (actualTorusRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (actualTorusRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);

            Vector3 direction = circleCenter - lastPoint;
            lastPoint = circleCenter;

            if (i == elbowSegments)
            {
                // last segment should always have the same orientation
                // as the next segment of the pipe
                direction = endPoint - point2;
            }
            else if (i == 0)
            {
                // first segment should always have the same orientation
                // as the how the previous segmented ended
                direction = point2 - startPoint;
            }

            GenerateCircleAtPoint(vertices, normals, circleCenter, direction);

            if (i > 0)
            {
                MakeElbowTriangles(vertices, triangles, i, index);
            }
        }
    }
    void GenerateCircleAtPoint(List<Vector3> vertices, List<Vector3> normals, Vector3 center, Vector3 direction)
    {
        // define a couple of utility variables to build circles
        float twoPi = Mathf.PI * 2;
        float radiansPerSegment = twoPi / pipeSegments;

        // generate two axes that define the plane with normal 'direction'
        // we use a plane to determine which direction we are moving in order
        // to ensure we are always using a left-hand coordinate system
        // otherwise, the triangles will be built in the wrong order and
        // all normals will end up inverted!
        Plane p = new Plane(Vector3.forward, Vector3.zero);
        Vector3 xAxis = Vector3.up;
        Vector3 yAxis = Vector3.right;
        if (p.GetSide(direction))
        {
            yAxis = Vector3.left;
        }

        // build left-hand coordinate system, with orthogonal and normalized axes
        Vector3.OrthoNormalize(ref direction, ref xAxis, ref yAxis);

        for (int i = 0; i < pipeSegments; i++)
        {
            Vector3 currentVertex =
                center +
                (pipeRadius * Mathf.Cos(radiansPerSegment * i) * xAxis) +
                (pipeRadius * Mathf.Sin(radiansPerSegment * i) * yAxis);
            vertices.Add(currentVertex);
            normals.Add((currentVertex - center).normalized);
        }
    }

    void MakeCylinderTriangles(List<int> triangles, int segmentIdx)
    {
        // connect the two circles corresponding to segment segmentIdx of the pipe
        int offset = segmentIdx * pipeSegments * 2;
        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + i + pipeSegments);
            triangles.Add(offset + i);

            triangles.Add(offset + (i + 1) % pipeSegments);
            triangles.Add(offset + (i + 1) % pipeSegments + pipeSegments);
            triangles.Add(offset + i + pipeSegments);
        }
    }
    void MakeElbowTriangles(List<Vector3> vertices, List<int> triangles, int segmentIdx, int elbowIdx)
    {
        // connect the two circles corresponding to segment segmentIdx of an
        // elbow with index elbowIdx
        int offset = (points.Count - 1) * pipeSegments * 2; // all vertices of cylinders
        offset += elbowIdx * (elbowSegments + 1) * pipeSegments; // all vertices of previous elbows
        offset += segmentIdx * pipeSegments; // the current segment of the current elbow

        // algorithm to avoid elbows strangling under dramatic
        // direction changes... we basically map vertices to the
        // one closest in the previous segment
        Dictionary<int, int> mapping = new Dictionary<int, int>();
        if (avoidStrangling)
        {
            List<Vector3> thisRingVertices = new List<Vector3>();
            List<Vector3> lastRingVertices = new List<Vector3>();

            for (int i = 0; i < pipeSegments; i++)
            {
                lastRingVertices.Add(vertices[offset + i - pipeSegments]);
            }

            for (int i = 0; i < pipeSegments; i++)
            {
                // find the closest one for each vertex of the previous segment
                Vector3 minDistVertex = Vector3.zero;
                float minDist = Mathf.Infinity;
                for (int j = 0; j < pipeSegments; j++)
                {
                    Vector3 currentVertex = vertices[offset + j];
                    float distance = Vector3.Distance(lastRingVertices[i], currentVertex);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        minDistVertex = currentVertex;
                    }
                }

                thisRingVertices.Add(minDistVertex);
                mapping.Add(i, vertices.IndexOf(minDistVertex));

            }
        }
        else
        {
            // keep current vertex order (do nothing)
            for (int i = 0; i < pipeSegments; i++)
            {
                mapping.Add(i, offset + i);
            }
        }

        // build triangles for the elbow segment
        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(mapping[i]);
            triangles.Add(offset + i - pipeSegments);
            triangles.Add(mapping[(i + 1) % pipeSegments]);

            triangles.Add(offset + i - pipeSegments);
            triangles.Add(offset + (i + 1) % pipeSegments - pipeSegments);
            triangles.Add(mapping[(i + 1) % pipeSegments]);
        }
    }
    void GenerateEndCaps(List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        // create the circular cap on each end of the pipe
        int firstCircleOffset = 0;
        int secondCircleOffset = (points.Count - 1) * pipeSegments * 2 - pipeSegments;

        vertices.Add(points[0]); // center of first segment cap
        int firstCircleCenter = vertices.Count - 1;
        normals.Add(points[0] - points[1]);

        vertices.Add(points[points.Count - 1]); // center of end segment cap
        int secondCircleCenter = vertices.Count - 1;
        normals.Add(points[points.Count - 1] - points[points.Count - 2]);

        for (int i = 0; i < pipeSegments; i++)
        {
            triangles.Add(firstCircleCenter);
            triangles.Add(firstCircleOffset + (i + 1) % pipeSegments);
            triangles.Add(firstCircleOffset + i);

            triangles.Add(secondCircleOffset + i);
            triangles.Add(secondCircleOffset + (i + 1) % pipeSegments);
            triangles.Add(secondCircleCenter);
        }
    }
}

