using UnityEditor;
using UnityEngine;

public class DebugControls : MonoBehaviour
{
    /// <summary>
    /// Listens for Key Presses for Toggling Editor Pause/Play
    /// </summary>
    /// 
    void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
		{
            if (Input.GetKey(KeyCode.LeftControl))
            {
				Debug.Break();
			}
		}
	}
}
