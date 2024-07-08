#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurveHelper))]
public class CurveHelperEditor : Editor
{
	public override void OnInspectorGUI()
	{
		CurveHelper curveHelper = (CurveHelper)target;

		DrawDefaultInspector();

		if (GUILayout.Button("Generate"))
		{
			curveHelper.Generate();
		}
	}
}
#endif