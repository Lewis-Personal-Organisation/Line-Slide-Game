using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class RoadMeshCreator : PathSceneTool
{
    public enum MaterialChoices
    {
        RoadUnderside,
        RoadInner
    }

    [Header("Road settings")]
    public float roadWidth = .4f;
    [Range(0, 1f)]
    public float thickness = .15f;
    public bool flattenSurface;
    [Space(5)]
    public Vector3 ExtendedVerticesHeight = new Vector3(0, 0.032f, 0);
    public bool showAllVerts = false;
    private Vector3[] allVerts = new Vector3[0];
    public bool showFrontFaceVerts = false;

    [Header("Additional Rendering Path Settings")]
    public bool renderStartAndEndFaces = false;
    public MaterialChoices materialChoice;
    public MeshFilter startMeshFilter;
    public MeshRenderer startMeshRenderer;
    public MeshFilter endMeshFilter;
    public MeshRenderer endMeshRenderer;

    [Header("Material settings")]
    public Material roadMaterial;
    public Material undersideMaterial;
    public float textureTiling = 1;

    [SerializeField]
    GameObject[] meshHolders = null;

    private MeshFilter[] meshFilters;
    private MeshRenderer[] meshRenderers;
    public Mesh[] meshes;

    public UnityAction playerScaleTrigger;

    [SerializeField] private RoadMeshCreator[] roadMeshSubscribers = new RoadMeshCreator[0];


    public void DrawPath()
    {
        StartCoroutine(PathUpdated());
    }
	public void DrawPathInstant()
    {
        if (pathCreator != null)
        {
            AssignMeshComponentsInstant();
            AssignMaterials();
            CreateRoadMesh();

            // When our mesh changes in editor, trigger our invoke
            if (playerScaleTrigger != null)
            {
                playerScaleTrigger.Invoke();
            }

            // For each subscriber, update and redraw its path
            for (int i = 0; i < roadMeshSubscribers.Length; i++)
            {
                if (roadMeshSubscribers[i] == null)
                    continue;

                roadMeshSubscribers[i].pathCreator.bezierPath = pathCreator.bezierPath;
                roadMeshSubscribers[i].DrawPath();
            }

            // Tell Unity to repaint
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }
    }

    protected override IEnumerator PathUpdated()
    {
        if (pathCreator != null)
        {
            yield return StartCoroutine(AssignMeshComponents());
            AssignMaterials();
            CreateRoadMesh();

            // When our mesh changes in editor, trigger our invoke
            if (playerScaleTrigger != null)
            {
                playerScaleTrigger.Invoke();
            }

            // For each subscriber, update and redraw its path
            for (int i = 0; i < roadMeshSubscribers.Length; i++)
            {
                if (roadMeshSubscribers[i] == null)
                    continue;

                roadMeshSubscribers[i].pathCreator.bezierPath = pathCreator.bezierPath;
                roadMeshSubscribers[i].DrawPathInstant();
            }

			// Tell Unity to repaint
#if UNITY_EDITOR
			SceneView.RepaintAll();
#endif
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showAllVerts)
        {
            Gizmos.color = Color.blue;
            for (int t = 0; t < allVerts.Length; t += 1)
            {
                Gizmos.DrawSphere(allVerts[t], 0.02F);
                Handles.Label(allVerts[t], new GUIContent($"v{t}"), new GUIStyle());
            }
        }
        else if (showFrontFaceVerts)
        {
            Gizmos.color = Color.red;

            if (startMeshFilter)
            {
                for (int t = 0; t < startMeshFilter.sharedMesh.vertices.Length; t += 1)
                {
                    Gizmos.DrawSphere(startMeshFilter.sharedMesh.vertices[t], 0.04F);
                    Handles.Label(startMeshFilter.sharedMesh.vertices[t], new GUIContent($"Vert {t}"), new GUIStyle());
                }
            }

            if (endMeshFilter)
            {
                for (int t = 0; t < endMeshFilter.sharedMesh.vertices.Length; t += 1)
                {
                    Gizmos.DrawSphere(endMeshFilter.sharedMesh.vertices[t], 0.04F);
                    Handles.Label(endMeshFilter.sharedMesh.vertices[t], new GUIContent($"Vert {t}"), new GUIStyle());
                }
            }
        }
    }
#endif

    void CreateRoadMesh()
    {
        allVerts = new Vector3[0];
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
            Vector3 localUp = usePathNormals ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
            Vector3 localRight = usePathNormals ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA + ExtendedVerticesHeight;
            verts[vertIndex + 1] = vertSideB + ExtendedVerticesHeight;

            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - localUp * thickness;
            verts[vertIndex + 3] = vertSideB - localUp * thickness;

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

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

        // If we want to show all verts in scene, assign the verts
        if (showAllVerts)
            allVerts = verts;

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

                StartCoroutine(BuildMeshes());
            }
        }

        // Rendering of Start and End Front faces
        if (renderStartAndEndFaces)
        {
            // The up and right vectors for offsets for the first path point index
            Vector3 up = usePathNormals ? Vector3.Cross(path.GetTangent(0), path.GetNormal(0)) : path.up;
            Vector3 right = usePathNormals ? path.GetNormal(0) : Vector3.Cross(up, path.GetTangent(0));
            Vector3 firstPathPoint = pathCreator.bezierPath.GetPoint(0);          // The first path point
            int[] triOrder = new int[] { 0, 1, 3, 0, 3, 2 };    // The rendering order for rendering two triangles 

            // Create Objects for rendering a mesh if they don't exist
            if (!startMeshFilter)
            {
                GameObject filterHolder = new GameObject("Start Face Mesh Holder");
                filterHolder.transform.SetParent(transform, true);
                startMeshFilter = filterHolder.AddComponent<MeshFilter>();
                startMeshRenderer = filterHolder.AddComponent<MeshRenderer>();
                startMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                startMeshRenderer.sharedMaterial = GetMaterialChoice(materialChoice);
            }
            else
            {
                if (startMeshRenderer.sharedMaterial != GetMaterialChoice(materialChoice))
                {
                    startMeshRenderer.sharedMaterial = GetMaterialChoice(materialChoice);
                }
            }

            if (!endMeshFilter)
            {
                GameObject filterHolder = new GameObject("End Face Mesh Holder");
                filterHolder.transform.SetParent(transform, true);
                endMeshFilter = filterHolder.AddComponent<MeshFilter>();
                endMeshRenderer = filterHolder.AddComponent<MeshRenderer>();
                endMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                endMeshRenderer.sharedMaterial = materialChoice == MaterialChoices.RoadInner ? roadMaterial : undersideMaterial;
            }
            else
            {
                if (endMeshRenderer.sharedMaterial != GetMaterialChoice(materialChoice))
                {
                    endMeshRenderer.sharedMaterial = GetMaterialChoice(materialChoice);
                }
            }

            // Creation of the mesh given the vertices location and vectors created above
            Mesh startmesh = new Mesh();
            startmesh.name = "Start Mesh";
            startmesh.vertices = new Vector3[] {
                firstPathPoint - right * Mathf.Abs(roadWidth) + ExtendedVerticesHeight,
                firstPathPoint + right * Mathf.Abs(roadWidth) + ExtendedVerticesHeight,
                firstPathPoint - right * Mathf.Abs(roadWidth) - up * thickness,
                firstPathPoint + right * Mathf.Abs(roadWidth) - up * thickness
            };
            startmesh.triangles = triOrder;
            startMeshFilter.sharedMesh = startmesh;

            // Reasign cached vectors for the last point in the Path
            up = usePathNormals ? Vector3.Cross(path.GetTangent(path.localPoints.Length - 1), path.GetNormal(path.localPoints.Length - 1)) : path.up;
            right = usePathNormals ? path.GetNormal(path.localPoints.Length - 1) : Vector3.Cross(up, path.GetTangent(path.localPoints.Length - 1));

            // End Mesh creation
            Mesh endMesh = new Mesh();
            endMesh.name = "End Mesh";
            endMesh.vertices = new Vector3[] {
                /*path.GetPoint(path.localPoints.Length - 1)*/ pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.points.Count - 1) - right * Mathf.Abs(roadWidth) + ExtendedVerticesHeight,
                /*path.GetPoint(path.localPoints.Length - 1)*/ pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.points.Count - 1) + right * Mathf.Abs(roadWidth) + ExtendedVerticesHeight,
                /*path.GetPoint(path.localPoints.Length - 1)*/ pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.points.Count - 1) - right * Mathf.Abs(roadWidth) - up * thickness,
                /*path.GetPoint(path.localPoints.Length - 1)*/ pathCreator.bezierPath.GetPoint(pathCreator.bezierPath.points.Count - 1) + right * Mathf.Abs(roadWidth) - up * thickness
            };
            endMesh.triangles = triOrder.Reverse().ToArray();
            endMeshFilter.sharedMesh = endMesh;
        }
        else
        {
            if (startMeshFilter && startMeshFilter.sharedMesh != null) startMeshFilter.mesh = null;
            if (endMeshFilter && endMeshFilter.sharedMesh != null) endMeshFilter.mesh = null;
        }
    }

    public IEnumerator BuildMeshes()
    {
        foreach (Mesh _mesh in meshes)
        {
            for (int i = 0; i < _mesh.vertices.Length; i++)
            {
                if (!float.IsNaN(_mesh.vertices[i].x))
                    yield break;

                yield return new WaitForEndOfFrame();
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
    }

    public void AssignMeshComponentsInstant()
    {
        if (meshHolders == null)
            return;

        meshRenderers = new MeshRenderer[meshHolders.Length];
        meshFilters = new MeshFilter[meshHolders.Length];
        meshes = new Mesh[meshHolders.Length];

        for (int x = 0; x < meshHolders.Length; x++)
        {
            if (meshHolders[x] == null)
            {
                break;
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

    void AssignMaterials()
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

    public Material GetMaterialChoice(MaterialChoices choice)
    {
        return choice == MaterialChoices.RoadInner ? roadMaterial : undersideMaterial;
    }
}