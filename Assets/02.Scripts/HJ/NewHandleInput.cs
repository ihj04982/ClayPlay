using UnityEngine;
using UnityEngine.UI;

public class NewHandleInput : MonoBehaviour
{

    public Transform target;
    public float force = 0f;
    MeshDeformer deformer;
    float forceOffset = 0.05f;
    public bool isClayStart = false;
    public bool isSwell = false;

    //public LineRenderer lineRenderer;
    public Image img;

    Shader standard;

    private void Start()
    {
        img.enabled = false;
        standard = Shader.Find("Standard");
    }
    private void Update()
    {
        if (isClayStart == true)
        {
            LayerMask layer = 1 << 12;
            RaycastHit hit;
            if (Physics.Raycast(target.position, target.right, out hit, 0.1f, layer))
            {

                float t = 0;
                t += Time.deltaTime / 3.0f;
                force = Mathf.Lerp(force, 0.03f, t);
                img.enabled = true;
                img.transform.position = hit.point;
                img.transform.forward = hit.normal;

                deformer = hit.collider.GetComponent<MeshDeformer>();
                
                Vector3 point = hit.point / 0.3f;
                if (isSwell == true)
                {
                    point -= hit.normal * forceOffset;
                }
                else
                {
                    point += hit.normal * forceOffset;
                }
                deformer.AddDeformingForce(point, force);
            }
            else
            {
                force = 0;
                img.enabled = false;
            }
        }
        if (this.transform.childCount != 0)
        {
            if (string.Compare(this.transform.GetChild(0).name, "Scene") == 0)
            {
                if (this.transform.GetChild(0).transform.childCount != 0)
                {
                    for (int i = 0; i < this.transform.GetChild(0).transform.childCount; i++)
                    {
                        if (this.transform.GetChild(0).transform.GetChild(i).GetComponent<MeshRenderer>() != null)
                        {
                            this.transform.GetChild(0).transform.GetChild(i).gameObject.AddComponent<MeshDeformer>();
                            this.transform.GetChild(0).transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
                            this.transform.GetChild(0).transform.GetChild(i).gameObject.layer = 12; //12¹ø  pipe
                            this.transform.GetChild(0).transform.GetChild(i).GetComponent<MeshRenderer>().material.shader = standard;
                            this.transform.GetChild(0).transform.GetChild(i).transform.SetParent(this.transform);
                        }
                    }
                }
                if (this.transform.GetChild(0).name == "Scene" && this.transform.GetChild(0).transform.childCount == 0)
                {
                    Destroy(this.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    public void Swelling()
    {
        isSwell = true;
    }
    public void Pressing()
    {
        isSwell = false;
    }
    public void OnClayStartClick(bool value)
    {
        if (value == true)
            isClayStart = true;
        else
            isClayStart = false;
    }

}
