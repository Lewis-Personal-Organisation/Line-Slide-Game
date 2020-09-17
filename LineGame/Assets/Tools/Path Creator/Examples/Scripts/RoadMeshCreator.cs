using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PathCreation.Examples
{
    public class RoadMeshCreator : PathSceneTool 
    {
        public enum vertDirections
        {
            Up,
            Down
        }

        [Header ("Road settings")]
        public float roadWidth = .4f;
        [Range (0, 1f)]
        public float thickness = .15f;
        public bool flattenSurface;
        [Space(5)]
        public bool enableExtendedVertices = false;
        public vertDirections extendedVerticesDirection;
        public Vector3 ExtendedVerticesHeight = new Vector3(0, 0.032f, 0);

        [Header ("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public float textureTiling = 1;

        [SerializeField]
        GameObject[] meshHolders;

        /*public*/ MeshFilter[] meshFilters;
        /*public*/ MeshRenderer[] meshRenderers;
        /*public*/ Mesh[] meshes;

        bool meshAssignmentDone = false;


        protected override IEnumerator PathUpdated() 
        {
            if (pathCreator != null) 
            {
                StartCoroutine(AssignMeshComponents());

                while(!meshAssignmentDone)
                {
                    yield return new WaitForEndOfFrame();
                }

                AssignMaterials();
                CreateRoadMesh();
            }
        }

        void CreateRoadMesh () 
        {
            Vector3[] verts = new Vector3[path.NumPoints * 8];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

            int vertIndex = 0;
            int triIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
            int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            for (int i = 0; i < path.NumPoints; i++) 
            {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross (path.GetTangent (i), path.GetNormal (i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal (i) : Vector3.Cross (localUp, path.GetTangent (i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint (i) - localRight * Mathf.Abs (roadWidth);
                Vector3 vertSideB = path.GetPoint (i) + localRight * Mathf.Abs (roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;

                // EXPERIMENTAL, Force render side vertices higher or lower than others
                if (enableExtendedVertices)
                {
                    verts[vertIndex + 0] -= (extendedVerticesDirection == vertDirections.Up) ? -ExtendedVerticesHeight : ExtendedVerticesHeight;
                    verts[vertIndex + 1] -= (extendedVerticesDirection == vertDirections.Up) ? -ExtendedVerticesHeight : ExtendedVerticesHeight;
                }

                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 4] = verts[vertIndex + 0];
                verts[vertIndex + 5] = verts[vertIndex + 1];
                verts[vertIndex + 6] = verts[vertIndex + 2];
                verts[vertIndex + 7] = verts[vertIndex + 3];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                uvs[vertIndex + 0] = new Vector2 (0, path.times[i]);
                uvs[vertIndex + 1] = new Vector2 (1, path.times[i]);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;

                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;

                // Sides of road normals
                normals[vertIndex + 4] = -localRight;
                normals[vertIndex + 5] = localRight;
                normals[vertIndex + 6] = -localRight;
                normals[vertIndex + 7] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop) 
                {
                    for (int j = 0; j < triangleMap.Length; j++) 
                    {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++) 
                    {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                    }
                }
                vertIndex += 8;
                triIndex += 6;
            }


            foreach (Mesh _mesh in meshes)
            {
                if (_mesh != null)
                {
                    _mesh.Clear();
                    _mesh.vertices = verts;
                    _mesh.uv = uvs;
                    _mesh.normals = normals;
                    _mesh.subMeshCount = 3;
                    _mesh.SetTriangles(roadTriangles, 0);
                    _mesh.SetTriangles(underRoadTriangles, 1);
                    _mesh.SetTriangles(sideOfRoadTriangles, 2);
                    _mesh.RecalculateBounds();
                }
            }
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        public IEnumerator AssignMeshComponents() 
        {
            

            meshRenderers = new MeshRenderer[meshHolders.Length];
            meshFilters = new MeshFilter[meshHolders.Length];
            meshes = new Mesh[meshHolders.Length];

            for (int x = 0; x < meshHolders.Length; x++)
            {
                if (meshHolders == null)
                {
                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    if (meshHolders[x] == null)
                    {
                        yield break;
                    }

                    meshHolders[x].transform.rotation = Quaternion.identity;
                    meshHolders[x].transform.position = Vector3.zero;
                    meshHolders[x].transform.localScale = Vector3.one;

                    if (!meshHolders[x].gameObject.GetComponent<MeshFilter>())
                    {
                        meshHolders[x].gameObject.AddComponent<MeshFilter>();
                    }

                    if (!meshHolders[x].gameObject.GetComponent<MeshRenderer>())
                    {
                        meshHolders[x].gameObject.AddComponent<MeshRenderer>();
                    }

                    meshRenderers[x] = meshHolders[x].GetComponent<MeshRenderer>();
                    meshFilters[x] = meshHolders[x].GetComponent<MeshFilter>();

                    if (meshes[x] == null)
                    {
                        meshes[x] = new Mesh();
                    }

                    meshFilters[x].sharedMesh = meshes[x];
                }
            }

            meshAssignmentDone = true;
        }

        void AssignMaterials () 
        {
            if (roadMaterial != null && undersideMaterial != null && meshRenderers[0] != null) 
            {
                foreach (MeshRenderer _renderer in meshRenderers)
                {
                    if (_renderer == null)
                        break;

                    _renderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
                    _renderer.sharedMaterials[0].mainTextureScale = new Vector3(1, textureTiling);
                }
            }
        }

    }
}