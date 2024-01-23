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

    // The Directory Path where the file will be created
    public static string directoryPath = "Assets/Scripts/Utility/Enums/EnumHolder/";
    public static string fileName = "ScenesEnum.cs";

    private static bool workingDirectoryExists => Directory.Exists(directoryPath);
    private static bool workingFileExists => File.Exists(directoryPath + fileName);

    private static StreamWriter streamWriterAtPath = new StreamWriter(directoryPath + fileName);


    // Generates our File at path
    public static void Generate()
    {
        try
        {
            if (!workingDirectoryExists)
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!workingFileExists)
            {
                File.Create(directoryPath + fileName).Dispose();
            }
        }
        catch (IOException ex)
        {
            Utils.Log(ex);
        }

        //The File and Folder must exist for this to function:
        using (streamWriterAtPath)
        {
            streamWriterAtPath.WriteLine("public enum " + enumName);
            streamWriterAtPath.WriteLine("{");

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                streamWriterAtPath.WriteLine("\t" + Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[i].path) + ",");
            }

            streamWriterAtPath.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
}
#endif