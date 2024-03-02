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
			rectPositioner.CachePosition();
		}
	}
}
#endif