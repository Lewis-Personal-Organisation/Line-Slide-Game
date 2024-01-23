#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RectPositioner))]
[CanEditMultipleObjects]
public class RectPositionerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		RectPositioner rectPositioner = (RectPositioner)target;

		DrawDefaultInspector();

		if (GUILayout.Button("Save Position"))
		{
			rectPositioner.values = Utils.InverseLerp(Vector2.zero, UnityEditor.Handles.GetMainGameViewSize(), (Vector2)rectPositioner.rectTransform.position) * 100F;
			rectPositioner.values.z = rectPositioner.rectTransform.position.z;
		}
	}
}
#endif