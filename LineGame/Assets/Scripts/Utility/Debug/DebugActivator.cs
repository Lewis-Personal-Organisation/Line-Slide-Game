using UnityEngine;

public class DebugActivator : MonoBehaviour
{
    public static DebugActivator instance;

    public Transform hiddenDebugControl;

    public bool isActive;


    private void Awake()
    {
        instance = this;
    }
}