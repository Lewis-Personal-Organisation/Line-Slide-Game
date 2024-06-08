using UnityEditor;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
    /// Attach to a GameObject to create a new path editor.

    public event System.Action pathUpdated;

    [SerializeField, HideInInspector]
    PathCreatorData editorData;

    public PathCreatorData EditorData
    {
        get { return editorData; }
    }

    [SerializeField, HideInInspector]
    bool initialized;

    GlobalDisplaySettings globalEditorDisplaySettings;

    // The Vertex path created from the current bezier path
    public VertexPath path
    {
        get
        {
            if (!initialized)
            {
                InitializeEditorData(false);
            }
            return editorData.GetVertexPath(transform);
        }
    }

    // The bezier path created in the editor
    public BezierPath bezierPath
    {
        get
        {
            if (!initialized)
            {
                InitializeEditorData(false);
            }
            return editorData.bezierPath;
        }
        set
        {
            if (!initialized)
            {
                InitializeEditorData(false);
            }
            editorData.bezierPath = value;
        }
    }

    #region Internal methods

    /// Used by the path editor to initialise some data
    public void InitializeEditorData(bool in2DMode)
    {
        if (editorData == null)
        {
            editorData = new PathCreatorData();
        }
        editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
        editorData.bezierOrVertexPathModified += TriggerPathUpdate;

        editorData.Initialize(in2DMode);
        initialized = true;
    }

    public void TriggerPathUpdate()
    {
        pathUpdated?.Invoke();
    }

#if UNITY_EDITOR

    // Draw the path when path objected is not selected (if enabled in settings)
    void OnDrawGizmos()
    {
        // Only draw path gizmo if the path object is not selected
        // (editor script is resposible for drawing when selected)
        GameObject selectedObj = UnityEditor.Selection.activeGameObject;
        if (selectedObj != gameObject)
        {
            if (path != null)
            {
                path.UpdateTransform(transform);

                if (globalEditorDisplaySettings == null)
                {
                    globalEditorDisplaySettings = GlobalDisplaySettings.Load();
                }

                if (globalEditorDisplaySettings.visibleWhenNotSelected)
                {
                    Gizmos.color = globalEditorDisplaySettings.bezierPath;

                    for (int i = 0; i < path.NumPoints; i++)
                    {
                        int nextI = i + 1;
                        if (nextI >= path.NumPoints)
                        {
                            if (path.isClosedLoop)
                            {
                                nextI %= path.NumPoints;
                            }
                            else
                            {
                                break;
                            }
                        }
                        Gizmos.DrawLine(path.GetPoint(i), path.GetPoint(nextI));
                    }
                }
            }
        }

        // UNFINISHED CODE
        //else
        //{
        //    Gizmos.color = Color.green;
        //    Vector3 pointAtTime = path.GetPointAtTime(timeOnPath, EndOfPathInstruction.Stop);
        //    Gizmos.DrawSphere(pointAtTime, .25F);
        //    EditorGUIUtility.systemCopyBuffer = $"UnityEditor.TransformWorldPlacementJSON:{{\"position\":{{\"x\":{pointAtTime.x},\"y\":{pointAtTime.y},\"z\":{pointAtTime.x}}},\"rotation\":{{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0}},\"scale\":{{\"x\":1.0,\"y\":1.0,\"z\":1.0}}}}";
        //    Debug.Log(EditorGUIUtility.systemCopyBuffer);
        //    VectorCache.controlPointOffset = pointAtTime;
        //    // Finish
        //}
    }
#endif
#endregion
}