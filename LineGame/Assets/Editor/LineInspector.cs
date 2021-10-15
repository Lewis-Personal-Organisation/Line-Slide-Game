using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Line))]
public class LineInspector : Editor
{
    Line line;

	Vector3 pA;
	Vector3 pB;


	private void OnSceneGUI()
    {
		line = target as Line;

		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? line.transform.rotation : Quaternion.identity;

		pA = line.transform.TransformPoint(line.pointA);
		pB = line.transform.TransformPoint(line.pointB);

		Handles.color = Color.blue;
		Handles.DrawLine(pA, pB);

		EditorGUI.BeginChangeCheck();
		pA = Handles.DoPositionHandle(pA, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			line.pointA = line.transform.InverseTransformPoint(pA);
		}

		EditorGUI.BeginChangeCheck();
		pB = Handles.DoPositionHandle(pB, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move Point");
			EditorUtility.SetDirty(line);
			line.pointB = line.transform.InverseTransformPoint(pB);
		}
	}
}
