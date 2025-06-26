using System.Collections.Generic;
using UnityEngine;

public class SliceMesh : MonoBehaviour
{
    public float radius = 5f;
    public float angle = 60f;
    public int segments = 30;
    public float duration = 0.5f;

    private float timer = 0f;
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Material material;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;  // Instance material so it's unique

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateFullSliceMesh();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / duration);

        Color col = material.color;
        col.a = alpha;
        material.color = col;

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    private void CreateFullSliceMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Center vertex
        vertices.Add(Vector3.zero);

        float halfAngle = angle / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
            float rad = Mathf.Deg2Rad * currentAngle;
            Vector3 vertex = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
            vertices.Add(vertex);
        }

        for (int i = 1; i <= segments; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
    }
}
