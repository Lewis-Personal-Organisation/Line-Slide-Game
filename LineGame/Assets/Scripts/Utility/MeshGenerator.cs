using UnityEngine;

[System.Serializable]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;

    public Material meshMat;

    [SerializeField]
    public struct triangle
    {
        public Vector3 a, b, c;
        public int mapx, mapy, mapz;

        public void Set()
        {
            mapx = 0;
            mapy = 1;
            mapz = 2;
        }
    }

    public triangle[] tris = new triangle[2];
    int index = 0;


    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = meshMat;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        triangle tri = new triangle();

        tri.a = new Vector3(0,0,0);
        tri.b = new Vector3(0,0,1);
        tri.c = new Vector3(1,0,0);
        tri.Set();

        tris[index] = tri;
        index++;

        triangle tri2 = new triangle();

        tri2.a = new Vector3(1, 0, 1);
        tri2.b = new Vector3(1, 0, 0);
        tri2.c = new Vector3(0, 0, 0);
        tri2.Set();

        tris[index] = tri2;
    }

    void UpdateMesh()
    {
        mesh.Clear();

        for (int x = 0; x < index + 1; x++)
        {
            mesh.vertices = new Vector3[]
            {
            tris[x].a,
            tris[x].b,
            tris[x].c
            };
            mesh.triangles = new int[]
            {
            tris[x].mapx,
            tris[x].mapy,
            tris[x].mapz
            };
        }

        mesh.RecalculateNormals();
    }
}
