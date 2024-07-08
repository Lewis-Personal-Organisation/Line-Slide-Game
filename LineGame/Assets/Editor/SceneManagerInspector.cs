using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(CustomSceneManager))]
public class SceneManagerInspector : Editor
{
	private SerializedProperty sIncludedScenes;
	private SerializedProperty sOptions;
	private SerializedProperty sSceneIndex;
	//int includedScenes = 0;
	//List<string> options = new List<string>();

	private void OnEnable()
	{
		sIncludedScenes = this.serializedObject.FindProperty("includedScenes");
		sOptions = this.serializedObject.FindProperty("options");
		sSceneIndex = this.serializedObject.FindProperty("sceneIndex");
	}

	public override void OnInspectorGUI()
	{
		this.serializedObject.Update();

		//SerializedProperty temp = sOptions.Copy();
		//Debug.Log($"options: {temp.arraySize}");
		//temp.Next(true);
		//temp.Next(true);
		//int arrayLength = temp.intValue;
		//temp.Next(true);
		//List<string> sceneOptions = new List<string>();
		//int lastindex = arrayLength - 1;
		//for (int i = 0; i < arrayLength; i++)
		//{
		//	sceneOptions.Add(temp.stringValue);
		//	if (i < lastindex) temp.Next(false);
		//}

		CustomSceneManager instance = (CustomSceneManager)target;
		Debug.Log($"options: {instance.options.Count}");

		int count = 0;
		bool mismatch = false;
		for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
		{
			if (EditorBuildSettings.scenes[i].enabled)
			{
				count++;
				if (!instance.options.Contains(Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path)))
				{
					mismatch = true;
					Debug.Log($"Mismatch!");
					break;
				}
			}
		}

		if (instance.includedScenes != count || mismatch)
		{
			Debug.Log($"Clearing {instance.includedScenes} {count} {mismatch}");
			instance.includedScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToList().Count;

			if (instance.sceneIndex > instance.includedScenes - 1)
			{
				Debug.Log("Scene index more than enabled scenes");
				instance.sceneIndex = 0;
			}

			instance.options.Clear();

			if (instance.includedScenes == 0)
				instance.options.Add("NO VALID SCENES");

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				if (EditorBuildSettings.scenes[i].enabled)
					instance.options.Add($"{Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path)}");
			}
		}

		instance.sceneIndex = EditorGUILayout.Popup("Scene To Load", instance.sceneIndex, instance.options.ToArray());
		DrawDefaultInspector();
		this.serializedObject.ApplyModifiedProperties();
	}
}