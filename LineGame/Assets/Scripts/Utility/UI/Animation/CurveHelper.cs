using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// easing functions from https://gist.github.com/Fonserbc/3d31a25e87fdaa541ddf

public enum CurveType
{
    Linear,
    Quadratic,
    Cubic,
    Quartic,
    Quintic,
    Sinusoidal,
    Exponential,
    Back,
    Bounce,
    Elastic,
    Circular,
    Count,
    NONE,
}
public enum CurveMode
{
    In,
    Out,
    InOut,
    Count,
}

public class CurveHelper : MonoBehaviour
{
    public CurveScriptableObject curveScriptableObjects;

	public static int NumKeyFrames = 60;
    public static float time = 1f;
    public static float Scalar = 1f;

    // Returns a float, requires two floats
    public static readonly Func<float, float>[,] EasingFunctionList = new Func<float, float>[(int)CurveType.Count, (int)CurveMode.Count];

#if UNITY_EDITOR
    public void Generate()
    {
        //Empty our lists before we regenerate
        curveScriptableObjects.linear.Clear();
        curveScriptableObjects.quadratic.Clear();
        curveScriptableObjects.cubic.Clear();
        curveScriptableObjects.quartic.Clear();
        curveScriptableObjects.quintic.Clear();
        curveScriptableObjects.sinusodial.Clear();
        curveScriptableObjects.exponential.Clear();
        curveScriptableObjects.back.Clear();
        curveScriptableObjects.bounce.Clear();
        curveScriptableObjects.elastic.Clear();
        curveScriptableObjects.circular.Clear();

        // Linear
        EasingFunctionList[(int)CurveType.Linear, (int)CurveMode.In] = EasingFunctions.Linear;
        curveScriptableObjects.linear.Add(GenerateCurve(NumKeyFrames, CurveType.Linear, CurveMode.In, Scalar, time));

        // Quadratic
        EasingFunctionList[(int)CurveType.Quadratic, (int)CurveMode.In] = EasingFunctions.Quadratic.In;
        EasingFunctionList[(int)CurveType.Quadratic, (int)CurveMode.Out] = EasingFunctions.Quadratic.Out;
        EasingFunctionList[(int)CurveType.Quadratic, (int)CurveMode.InOut] = EasingFunctions.Quadratic.InOut;
        curveScriptableObjects.quadratic.Add(GenerateCurve(NumKeyFrames, CurveType.Quadratic, CurveMode.In, Scalar, time));
        curveScriptableObjects.quadratic.Add(GenerateCurve(NumKeyFrames, CurveType.Quadratic, CurveMode.Out, Scalar, time));
        curveScriptableObjects.quadratic.Add(GenerateCurve(NumKeyFrames, CurveType.Quadratic, CurveMode.InOut, Scalar, time));

        // Cubic    
        EasingFunctionList[(int)CurveType.Cubic, (int)CurveMode.In] = EasingFunctions.Cubic.In;
        EasingFunctionList[(int)CurveType.Cubic, (int)CurveMode.Out] = EasingFunctions.Cubic.Out;
        EasingFunctionList[(int)CurveType.Cubic, (int)CurveMode.InOut] = EasingFunctions.Cubic.InOut;
        curveScriptableObjects.cubic.Add(GenerateCurve(NumKeyFrames, CurveType.Cubic, CurveMode.In, Scalar, time));
        curveScriptableObjects.cubic.Add(GenerateCurve(NumKeyFrames, CurveType.Cubic, CurveMode.Out, Scalar, time));
        curveScriptableObjects.cubic.Add(GenerateCurve(NumKeyFrames, CurveType.Cubic, CurveMode.InOut, Scalar, time));

        // Quartic
        EasingFunctionList[(int)CurveType.Quartic, (int)CurveMode.In] = EasingFunctions.Quartic.In;
        EasingFunctionList[(int)CurveType.Quartic, (int)CurveMode.Out] = EasingFunctions.Quartic.Out;
        EasingFunctionList[(int)CurveType.Quartic, (int)CurveMode.InOut] = EasingFunctions.Quartic.InOut;
        curveScriptableObjects.quartic.Add(GenerateCurve(NumKeyFrames, CurveType.Quartic, CurveMode.In, Scalar, time));
        curveScriptableObjects.quartic.Add(GenerateCurve(NumKeyFrames, CurveType.Quartic, CurveMode.Out, Scalar, time));
        curveScriptableObjects.quartic.Add(GenerateCurve(NumKeyFrames, CurveType.Quartic, CurveMode.InOut, Scalar, time));

        // Quintic
        EasingFunctionList[(int)CurveType.Quintic, (int)CurveMode.In] = EasingFunctions.Quintic.In;
        EasingFunctionList[(int)CurveType.Quintic, (int)CurveMode.Out] = EasingFunctions.Quintic.Out;
        EasingFunctionList[(int)CurveType.Quintic, (int)CurveMode.InOut] = EasingFunctions.Quintic.InOut;
        curveScriptableObjects.quintic.Add(GenerateCurve(NumKeyFrames, CurveType.Quintic, CurveMode.In, Scalar, time));
        curveScriptableObjects.quintic.Add(GenerateCurve(NumKeyFrames, CurveType.Quintic, CurveMode.Out, Scalar, time));
        curveScriptableObjects.quintic.Add(GenerateCurve(NumKeyFrames, CurveType.Quintic, CurveMode.InOut, Scalar, time));

        // Sinusodial
        EasingFunctionList[(int)CurveType.Sinusoidal, (int)CurveMode.In] = EasingFunctions.Sinusoidal.In;
        EasingFunctionList[(int)CurveType.Sinusoidal, (int)CurveMode.Out] = EasingFunctions.Sinusoidal.Out;
        EasingFunctionList[(int)CurveType.Sinusoidal, (int)CurveMode.InOut] = EasingFunctions.Sinusoidal.InOut;
        curveScriptableObjects.sinusodial.Add(GenerateCurve(NumKeyFrames, CurveType.Sinusoidal, CurveMode.In, Scalar, time));
        curveScriptableObjects.sinusodial.Add(GenerateCurve(NumKeyFrames, CurveType.Sinusoidal, CurveMode.Out, Scalar, time));
        curveScriptableObjects.sinusodial.Add(GenerateCurve(NumKeyFrames, CurveType.Sinusoidal, CurveMode.InOut, Scalar, time));

        // Exponential
        EasingFunctionList[(int)CurveType.Exponential, (int)CurveMode.In] = EasingFunctions.Exponential.In;
        EasingFunctionList[(int)CurveType.Exponential, (int)CurveMode.Out] = EasingFunctions.Exponential.Out;
        EasingFunctionList[(int)CurveType.Exponential, (int)CurveMode.InOut] = EasingFunctions.Exponential.InOut;
        curveScriptableObjects.exponential.Add(GenerateCurve(NumKeyFrames, CurveType.Exponential, CurveMode.In, Scalar, time));
        curveScriptableObjects.exponential.Add(GenerateCurve(NumKeyFrames, CurveType.Exponential, CurveMode.Out, Scalar, time));
        curveScriptableObjects.exponential.Add(GenerateCurve(NumKeyFrames, CurveType.Exponential, CurveMode.InOut, Scalar, time));

        // Back
        EasingFunctionList[(int)CurveType.Back, (int)CurveMode.In] = EasingFunctions.Back.In;
        EasingFunctionList[(int)CurveType.Back, (int)CurveMode.Out] = EasingFunctions.Back.Out;
        EasingFunctionList[(int)CurveType.Back, (int)CurveMode.InOut] = EasingFunctions.Back.InOut;
        curveScriptableObjects.back.Add(GenerateCurve(NumKeyFrames, CurveType.Back, CurveMode.In, Scalar, time));
        curveScriptableObjects.back.Add(GenerateCurve(NumKeyFrames, CurveType.Back, CurveMode.Out, Scalar, time));
        curveScriptableObjects.back.Add(GenerateCurve(NumKeyFrames, CurveType.Back, CurveMode.InOut, Scalar, time));

        // Bounce
        EasingFunctionList[(int)CurveType.Bounce, (int)CurveMode.In] = EasingFunctions.Bounce.In;
        EasingFunctionList[(int)CurveType.Bounce, (int)CurveMode.Out] = EasingFunctions.Bounce.Out;
        EasingFunctionList[(int)CurveType.Bounce, (int)CurveMode.InOut] = EasingFunctions.Bounce.InOut;
        curveScriptableObjects.bounce.Add(GenerateCurve(NumKeyFrames, CurveType.Bounce, CurveMode.In, Scalar, time));
        curveScriptableObjects.bounce.Add(GenerateCurve(NumKeyFrames, CurveType.Bounce, CurveMode.Out, Scalar, time));
        curveScriptableObjects.bounce.Add(GenerateCurve(NumKeyFrames, CurveType.Bounce, CurveMode.InOut, Scalar, time));

        // Elastic
        EasingFunctionList[(int)CurveType.Elastic, (int)CurveMode.In] = EasingFunctions.Elastic.In;
        EasingFunctionList[(int)CurveType.Elastic, (int)CurveMode.Out] = EasingFunctions.Elastic.Out;
        EasingFunctionList[(int)CurveType.Elastic, (int)CurveMode.InOut] = EasingFunctions.Elastic.InOut;
        curveScriptableObjects.elastic.Add(GenerateCurve(NumKeyFrames, CurveType.Elastic, CurveMode.In, Scalar, time));
        curveScriptableObjects.elastic.Add(GenerateCurve(NumKeyFrames, CurveType.Elastic, CurveMode.Out, Scalar, time));
        curveScriptableObjects.elastic.Add(GenerateCurve(NumKeyFrames, CurveType.Elastic, CurveMode.InOut, Scalar, time));

        // Elastic
        EasingFunctionList[(int)CurveType.Circular, (int)CurveMode.In] = EasingFunctions.Circular.In;
        EasingFunctionList[(int)CurveType.Circular, (int)CurveMode.Out] = EasingFunctions.Circular.Out;
        EasingFunctionList[(int)CurveType.Circular, (int)CurveMode.InOut] = EasingFunctions.Circular.InOut;
        curveScriptableObjects.circular.Add(GenerateCurve(NumKeyFrames, CurveType.Circular, CurveMode.In, Scalar, time));
        curveScriptableObjects.circular.Add(GenerateCurve(NumKeyFrames, CurveType.Circular, CurveMode.Out, Scalar, time));
        curveScriptableObjects.circular.Add(GenerateCurve(NumKeyFrames, CurveType.Circular, CurveMode.InOut, Scalar, time));

        EditorUtility.SetDirty(curveScriptableObjects);
    }

    /// <summary>
    /// Returns an Animation Curve given the type and mode, quality level (keyframe count), scale and time
    /// </summary>
    /// <param name="keyFrameCount"></param>
    /// <param name="curveType"></param>
    /// <param name="curveMode"></param>
    /// <param name="scalar"></param>
    /// <param name="animTime"></param>
    /// <returns></returns>
	public static AnimationCurve GenerateCurve(int keyFrameCount, CurveType curveType, CurveMode curveMode, float scalar, float animTime)
    {
        Keyframe[] keys = new Keyframe[keyFrameCount];
        Func<float, float> function = EasingFunctionList[(int)curveType, (int)curveMode];

        for (int i = 0; i < NumKeyFrames; i++)
        {
            // The point in time between 0 and 1
            float timeFrac = (float)i / (NumKeyFrames - 1);

			// Assign a scaled time fraction, and scaled value
			keys[i] = new Keyframe(animTime * timeFrac, function(timeFrac) * scalar);
        }

        AnimationCurve curve = new AnimationCurve(keys);

        for (int i = 0; i < keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        return curve;
    }
#endif

	public float Evaluate(CurveType curveType, CurveMode curveMode, float time)
	{
        switch (curveType)
		{
			case CurveType.Linear:
                return curveScriptableObjects.linear[(int)curveMode].Evaluate(time);
			case CurveType.Quadratic:
                return curveScriptableObjects.quadratic[(int)curveMode].Evaluate(time);
            case CurveType.Cubic:
                return curveScriptableObjects.cubic[(int)curveMode].Evaluate(time);
            case CurveType.Quartic:
                return curveScriptableObjects.quartic[(int)curveMode].Evaluate(time);
            case CurveType.Quintic:
                return curveScriptableObjects.quintic[(int)curveMode].Evaluate(time);
            case CurveType.Sinusoidal:
                return curveScriptableObjects.sinusodial[(int)curveMode].Evaluate(time);
            case CurveType.Exponential:
                return curveScriptableObjects.exponential[(int)curveMode].Evaluate(time);
            case CurveType.Back:
                return curveScriptableObjects.back[(int)curveMode].Evaluate(time);
            case CurveType.Bounce:
                return curveScriptableObjects.bounce[(int)curveMode].Evaluate(time);
            case CurveType.Elastic:
                return curveScriptableObjects.elastic[(int)curveMode].Evaluate(time);
            case CurveType.Circular:
                return curveScriptableObjects.circular[(int)curveMode].Evaluate(time);
            case CurveType.Count:
            case CurveType.NONE:
                return float.MaxValue;
		}
        return float.MaxValue;
	}
}