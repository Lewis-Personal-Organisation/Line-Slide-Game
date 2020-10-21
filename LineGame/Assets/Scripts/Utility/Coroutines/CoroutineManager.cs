using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager inst;

    public static Hashtable routineList = new Hashtable();


    public static float GameTime
    {
        get
        {
            return Time.time;
        }
    }

    public static float RoutineCount
    {
        get
        {
            return routineList.Count;
        }
    }


    public void Awake()
    {
        inst = this;
    }

    public static void Start(IEnumerator _enum)
    {
        routineList.Add(_enum.GetHashCode(), _enum);
        inst.StartCoroutine(_enum);
    }

    public static void Stop(IEnumerator _enum)
    {
        if (_enum is null)
            return;

        inst.StopCoroutine(_enum);
        routineList.Remove(_enum.GetHashCode());
    }
}