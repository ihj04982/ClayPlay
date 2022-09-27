using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshCollider))]
public class MeshDeformer : MonoBehaviour
{

    float springForce = 0.05f;
    float damping = 5f;
    Mesh deformingMesh;
    public Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;

    float uniformScale = 1f;
    public Vector3 pointToVertex;
    public bool isSqueeze = false;
    NewHandleInput handleInput;

    private void Start()
    {
        handleInput = GameObject.Find("ClayManager").GetComponent<NewHandleInput>();
        GetMesh();

    }
    public void GetMesh()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        if (deformingMesh != null)
        {
            //uniformScale = transform.localScale.x;
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                UpdateVertex(i);
            }
            deformingMesh.vertices = displacedVertices;
            deformingMesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = deformingMesh;
        }
    }

    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        //displacement *= uniformScale;

        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;

        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (Time.deltaTime);
    }


    public void AddDeformingForce(Vector3 point, float force)
    {
        //point = transform.InverseTransformPoint(point);
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
            //Debug.Log(point);
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        if (handleInput != null)
        {
            pointToVertex = (displacedVertices[i] - point);
            float attenuatedForce = force / (pointToVertex.sqrMagnitude);
            float velocity = attenuatedForce * Time.deltaTime;
            if (handleInput.isSwell== true)
            {
                vertexVelocities[i] += pointToVertex.normalized * 0.15f * velocity;
            }
            else
            {
                vertexVelocities[i] += pointToVertex.normalized * 0.15f* velocity;
            }
        }
    }
}

