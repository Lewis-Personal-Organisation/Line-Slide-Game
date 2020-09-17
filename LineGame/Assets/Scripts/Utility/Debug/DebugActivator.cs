using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugActivator : MonoBehaviour
{
    public static DebugActivator instance;

    public bool isActive;


    private void Awake()
    {
        instance = this;
    }

    public Transform hiddenDebugControl;
}
