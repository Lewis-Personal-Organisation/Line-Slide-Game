using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

[CustomEditor(typeof(CustomSceneManager))]
public class SceneManagerInspector : Editor
{
	int includedScenes = 0;
	List<string> options = new List<string>();

	public override void OnInspectorGUI()
    {
		CustomSceneManager instance = (CustomSceneManager)target;

		int count = 0;
		bool mismatch = false;
		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			if (EditorBuildSettings.scenes[i].enabled)
			{
				count++;
				if (!options.Contains(Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path)))
				{
					mismatch = true;
					break;
				}
			}
		}

		if (includedScenes != count || mismatch)
		{
			includedScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToList().Count;

			if (instance.sceneIndex > includedScenes - 1)
				instance.sceneIndex = 0;

			options.Clear();

			if (includedScenes == 0)
				options.Add("NO VALID SCENES");

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				if (EditorBuildSettings.scenes[i].enabled)
					options.Add($"{Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path)}");
			}
		}

		instance.sceneIndex = EditorGUILayout.Popup("Scene To Load", instance.sceneIndex, options.ToArray());
		DrawDefaultInspector();
	}
}