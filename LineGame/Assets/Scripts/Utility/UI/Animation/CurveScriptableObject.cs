using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Curve Data", menuName = "Animation Curve Scriptable Object")]
public class CurveScriptableObject : ScriptableObject
{
    public List<AnimationCurve> linear = new List<AnimationCurve>();
    public List<AnimationCurve> quadratic = new List<AnimationCurve>();
    public List<AnimationCurve> cubic = new List<AnimationCurve>();
    public List<AnimationCurve> quartic = new List<AnimationCurve>();
    public List<AnimationCurve> quintic = new List<AnimationCurve>();
    public List<AnimationCurve> sinusodial = new List<AnimationCurve>();
    public List<AnimationCurve> exponential = new List<AnimationCurve>();
    public List<AnimationCurve> back = new List<AnimationCurve>();
    public List<AnimationCurve> bounce = new List<AnimationCurve>();
    public List<AnimationCurve> elastic = new List<AnimationCurve>();
    public List<AnimationCurve> circular = new List<AnimationCurve>();
}
