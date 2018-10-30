using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePieces : MonoBehaviour
{

    public float thickness;

    private static uint pieceNum = 0;

    Mesh GenerateMesh(Vector3[] Vertices)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[Vertices.Length * 6];

        int baseTriangleNum = (Vertices.Length - 2) * 2;
        int triangleNum = baseTriangleNum + Vertices.Length * 2;
        int[] triangles = new int[triangleNum * 3];

        float halfThickness = thickness / 2.0f;

        for (uint i = 0; i < Vertices.Length; ++i)
        {
            vertices[i] = Vertices[i];
            vertices[i].y += halfThickness;
            vertices[i + Vertices.Length] = Vertices[i];
            vertices[i + Vertices.Length].y -= halfThickness;

            vertices[i + Vertices.Length * 2] = vertices[i];
            vertices[i + Vertices.Length * 3] = vertices[i + Vertices.Length];
            vertices[i + Vertices.Length * 4] = vertices[i];
            vertices[i + Vertices.Length * 5] = vertices[i + Vertices.Length];
        }

        for (int i = 0; i < Vertices.Length - 2; ++i)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 2;
            triangles[i * 3 + 2] = i + 1;

            triangles[(i + Vertices.Length - 2) * 3 + 0] = Vertices.Length;
            triangles[(i + Vertices.Length - 2) * 3 + 1] = Vertices.Length + i + 1;
            triangles[(i + Vertices.Length - 2) * 3 + 2] = Vertices.Length + i + 2;
        }

        for (int i = 0; i < Vertices.Length - 1; ++i)
        {
            triangles[(baseTriangleNum + i * 2) * 3 + 0] = Vertices.Length * 2 + i;
            triangles[(baseTriangleNum + i * 2) * 3 + 1] = Vertices.Length * 4 + i + 1;
            triangles[(baseTriangleNum + i * 2) * 3 + 2] = Vertices.Length * 2 + i + Vertices.Length;

            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 0] = Vertices.Length * 2 + i + Vertices.Length;
            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 1] = Vertices.Length * 4 + i + 1;
            triangles[(baseTriangleNum + i * 2 + 1) * 3 + 2] = Vertices.Length * 4 + i + Vertices.Length + 1;
        }

        triangles[(triangleNum - 2) * 3 + 0] = Vertices.Length * 2 + Vertices.Length - 1;
        triangles[(triangleNum - 2) * 3 + 1] = Vertices.Length * 4 + 0;
        triangles[(triangleNum - 2) * 3 + 2] = Vertices.Length * 2 + Vertices.Length - 1 + Vertices.Length;

        triangles[(triangleNum - 1) * 3 + 0] = Vertices.Length * 2 + Vertices.Length - 1 + Vertices.Length;
        triangles[(triangleNum - 1) * 3 + 1] = Vertices.Length * 4 + 0;
        triangles[(triangleNum - 1) * 3 + 2] = Vertices.Length * 4 + Vertices.Length;

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

        piece.AddComponent<Rigidbody>();
        piece.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

        piece.AddComponent<MeshRenderer>();

        return piece;
    }
}
