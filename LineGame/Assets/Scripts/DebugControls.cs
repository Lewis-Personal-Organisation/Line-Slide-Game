using UnityEditor;
using UnityEngine;

public class DebugControls : MonoBehaviour
{
    /// <summary>
    /// Listens for Key Presses for Toggling Editor Pause/Play
    /// </summary>
    /// 

    [ExecuteAlways]
    void Update()
	{
		Debug.Log(Utils.ColourText($"Executing", Color.cyan));
		if (Application.isPlaying && Application.isEditor)
        {
            if (Input.GetKey(KeyCode.Pause))
            {
                Debug.Break();
            }
        }
    }
}