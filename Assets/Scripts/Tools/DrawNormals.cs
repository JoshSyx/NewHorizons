using UnityEngine;

public class DrawNormals : MonoBehaviour
{
    public float normalLength = 0.5f;

    void OnDrawGizmos()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mf) return;
        Mesh mesh = mf.sharedMesh;
        if (!mesh) return;

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            Vector3 worldNormal = transform.TransformDirection(normals[i]);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(worldPos, worldPos + worldNormal * normalLength);
        }
    }
}