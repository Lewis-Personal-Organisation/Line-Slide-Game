using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvedLine : MonoBehaviour
{
    public Vector3[] points;


    public void Reset()
    {
        points = new Vector3[]
        {
            new Vector3(1F, 0F, 0F),
            new Vector3(2F, 0F, 0F),
            new Vector3(3F, 0F, 0F)
        };
    }

    public Vector3 GetPoint(float _t)
    {
        return transform.TransformPoint(GetBezierPoint(points[0], points[1], points[2], _t));
    }

    public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }
}
