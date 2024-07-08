#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColliderGenerator))]
[CanEditMultipleObjects]
public class ColliderGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ColliderGenerator colliderGenerator = (ColliderGenerator)target;
		
		if (GUILayout.Button("Generate"))
		{
			colliderGenerator.Generate();
		}

		DrawDefaultInspector();
	}
}
#endif