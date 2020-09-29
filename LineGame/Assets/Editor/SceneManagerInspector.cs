using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomSceneManager))]
public class SceneManagerInspector : Editor
{
    CustomSceneManager instance = null;

    public override void OnInspectorGUI()
    {
        instance = (CustomSceneManager)target;
        
        if (GUILayout.Button("Refresh Scene List"))
        {
            EnumGenerator.Generate();
        }

        DrawDefaultInspector();
    }
}