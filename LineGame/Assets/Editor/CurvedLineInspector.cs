using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurvedLine))]
public class CurvedLineInspector : Editor
{
    private CurvedLine curvedLine; 
    private Transform handleTransform;
    private Quaternion handleRotation;
        
    private const int lineSteps = 10;


    private void OnSceneGUI()
    {
        curvedLine = (CurvedLine)target;

        handleTransform = curvedLine.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

        Vector3 pointX = ShowPoint(0);
        Vector3 pointY = ShowPoint(1);
        Vector3 pointZ = ShowPoint(2);

        Handles.color = Color.gray;
        Handles.DrawLine(pointX, pointY);
        Handles.DrawLine(pointY, pointZ);

        Handles.color = Color.white;
        Vector3 lineStart = curvedLine.GetPoint(0);
        for (int i = 1; i <= lineSteps; i++)
        {
            Vector3 lineEnd = curvedLine.GetPoint(i /lineSteps);
            Handles.DrawLine(lineStart, lineEnd);
            lineStart = lineEnd;
        }
    }

    private Vector3 ShowPoint(int _index)
    {
        Vector3 point = handleTransform.TransformPoint(curvedLine.points[_index]);

        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, handleRotation); 
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvedLine, "Move Point");
            EditorUtility.SetDirty(curvedLine);
            curvedLine.points[_index] =  handleTransform.InverseTransformPoint(point);
        }
        return point;
    }
}
