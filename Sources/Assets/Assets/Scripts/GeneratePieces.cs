using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePieces : MonoBehaviour
{

    public float thickness;

    Mesh GenerateMesh(Vector3[] Vertices)
    {
        int vNum = Vertices.Length;
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[vNum * 6];

        int baseTriangleNum = (vNum - 2) * 2;
        int triangleNum = baseTriangleNum + vNum * 2;
        int[] triangles = new int[triangleNum * 3];

        float halfThickness = thickness / 2.0f;

        for (uint i = 0; i < vNum; ++i)
        {
            vertices[i] = Vertices[i];
            vertices[i].y += halfThickness;
            vertices[i + vNum] = Vertices[i];
            vertices[i + vNum].y -= halfThickness;

            vertices[i + vNum * 2] = vertices[i];
            vertices[i + vNum * 3] = vertices[i + vNum];
            vertices[i + vNum * 4] = vertices[i];
            vertices[i + vNum * 5] = vertices[i + vNum];
        }

        for (int i = 0; i < vNum - 2; ++i)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;

            triangles[(i + vNum - 2) * 3 + 0] = vNum;
            triangles[(i + vNum - 2) * 3 + 1] = vNum + i + 1;
            triangles[(i + vNum - 2) * 3 + 2] = vNum + i + 2;
        }

        for (int i = 0; i < vNum - 1; ++i)
        {
            triangles[(baseTriangleNum + i * 2) * 3 + 0] = vNum * 2 + i;
            triangles[(baseTriangleNum + i * 2) * 3 + 1] = vNum * 4 + i + 1;
            triangles[(baseTriangleNum + i * 2) * 3 + 2] = vNum * 2 + i + vNum;

            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 0] = vNum * 2 + i + vNum;
            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 1] = vNum * 4 + i + 1;
            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 2] = vNum * 4 + i + vNum + 1;
        }

        triangles[(triangleNum - 2) * 3 + 0] = vNum * 2 + vNum - 1;
        triangles[(triangleNum - 2) * 3 + 1] = vNum * 4 + 0;
        triangles[(triangleNum - 2) * 3 + 2] = vNum * 2 + vNum - 1 + vNum;

        triangles[(triangleNum - 1) * 3 + 0] = vNum * 2 + vNum - 1 + vNum;
        triangles[(triangleNum - 1) * 3 + 1] = vNum * 4 + 0;
        triangles[(triangleNum - 1) * 3 + 2] = vNum * 4 + vNum;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }

    public GameObject CreatePiece(Vector3[] Vertices)
    {
        GameObject piece = new GameObject();

        MeshFilter meshFilter = piece.AddComponent<MeshFilter>() as MeshFilter;
        MeshCollider meshCollider = piece.AddComponent<MeshCollider>() as MeshCollider;
        Mesh mesh = GenerateMesh(Vertices);

        meshFilter.mesh = mesh;
        meshCollider.convex = true;
        meshCollider.sharedMesh = mesh;

        Rigidbody rigidbody = piece.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

        piece.AddComponent<MeshRenderer>();

        return piece;
    }
}
