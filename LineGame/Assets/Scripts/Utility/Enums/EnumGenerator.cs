// Author: Lewis Dawson
// Description: Generates a Scene Enum (.cs file) using the Scenes Ticked in the Build Window of Unity.
// The accompanying files CustomSceneManager.cs and SceneManagerInspector must be use accordingly:
// SceneManagerInspector must be placed in the Assets/Editor Folder. CustomSceneManager must be placed in the 
// Scene Heirarchy.
// Use: Click the "Refresh Scene List" button on the CustomSceneManager script to Update the list of selectable scenes

#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class EnumGenerator
{
    // The Name of our Enum
    public static string enumName = "Scenes";

    // The FileName of our Enum. This doesn't matter as the enum will be publicly assessable using EnumName
    public static string enumFileName = "ScenesEnum";

    // The Directory Path where the file will be created
    public static string directoryPath = "Assets/Scripts/Utility/Enums/EnumHolder/";
    public static string fileName = $"{enumFileName}.cs";


    // Generates our File at path
    public static void Generate()
    {
        // If our Directory Path does not exist, Create the Path and the File. If the Path existed but no file, create the file
        // If both exist, we don't need to create anything, but we do need to overwrite its contents
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(directoryPath + fileName))
            {
                File.Create(directoryPath + fileName).Dispose();
            }
        }
        catch (IOException ex)
        {
            UnityEngine.Debug.Log(ex);
        }

        //The File and Folder must exist for this to function:
        using (StreamWriter _sw = new StreamWriter(directoryPath + fileName))
        {
            _sw.WriteLine("public enum " + enumName);
            _sw.WriteLine("{");

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                _sw.WriteLine("\t" + Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path) + ",");
            }

            _sw.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}
#endif