using System.Collections;
using System.Collections.Generic;
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
