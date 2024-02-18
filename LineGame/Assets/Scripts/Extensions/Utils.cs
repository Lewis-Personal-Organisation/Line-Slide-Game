using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngineInternal;

public static class Utils
{	
	public static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);

	public static Vector3 InverseLerp(Vector3 a, Vector3 b, Vector3 t)
	{
		return new Vector3(
		InverseLerp(a.x, b.x, t.x),
		InverseLerp(a.y, b.y, t.y),
		InverseLerp(a.z, b.z, t.z)
		);
	}

	public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 t)
	{
		return new Vector3(
		InverseLerp(a.x, b.x, t.x),
		InverseLerp(a.y, b.y, t.y)
		);
	}

	public enum Axis
	{
		X,
		Y,
		Z
	}

	/// <summary>
	/// Returns the original Vector3 with a specific axis value replaced
	/// </summary>
	/// <param name="original"></param>
	/// <param name="selected"></param>
	/// <returns></returns>
	public static Vector3 Replace(this Vector3 original, Axis selected, float newValue)
	{
		switch (selected)
		{
			case Axis.X:
				original.x = newValue;
				break;
			case Axis.Y:
				original.y = newValue;
				break;
			case Axis.Z:
				original.z = newValue;
				break;
		}

		return original;
	}

	///// <summary>
	///// Returns a Percentage of where a RectTransform sits in respective to a Canvas
	///// </summary>
	///// <returns></returns>
	//public static Vector3 CanvasWorldPosPercentage(this RectTransform rectt)
	//{

	//}

	/// <summary>
	/// Convert a bool to an integer where true == 1 and false = -1
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static int BoolToInt(bool value)
	{
		return value ? 1 : -1;
	}

	/// <summary>
	/// Get the With of the Orthographic Camera
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static float GetOrthoCamWidth(this Camera camera)
	{
		return 2f * camera.orthographicSize * camera.aspect;
	}

	/// <summary>
	/// Get the Height of the Orthographic Camera
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static float GetOrthoCamHeight(this Camera camera)
	{
		return 2f * camera.orthographicSize;
	}

	/// <summary>
	/// Get the Size of an Orthographic Camera
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static Vector2 GetOrthoCamSize(this Camera camera)
	{
		return new Vector2(2f * camera.orthographicSize * camera.aspect, 2f * camera.orthographicSize);
	}

	/// <summary>
	/// Returns a ray originating in the Bottom Left of the Camera View Fustrum
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static Ray CameraViewportBL(this Camera camera)
	{
		return camera.ViewportPointToRay(new Vector3(0, 0, 0));
	}
	/// <summary>
	///  Returns a ray originating in the Top Left of the Camera View Fustrum
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static Ray CameraViewportTL(this Camera camera)
	{
		return camera.ViewportPointToRay(new Vector3(0, 1, 0));
	}

	/// <summary>
	///  Returns a ray originating in the Top Right of the Camera View Fustrum
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static Ray CameraViewportTR(this Camera camera)
	{
		return camera.ViewportPointToRay(new Vector3(1, 1, 0));
	}
	/// <summary>
	///  Returns a ray originating in the Bottom Right of the Camera View Fustrum
	/// </summary>
	/// <param name="camera"></param>
	/// <returns></returns>
	public static Ray CameraViewportBR(this Camera camera)
	{
		return camera.ViewportPointToRay(new Vector3(1, 0, 0));
	}

	public static Vector3 ViewportTL(this Camera camera)
	{
		return camera.ViewportToWorldPoint(new Vector3(0, 1, 0));
	}
	public static Vector3 ViewportTR(this Camera camera)
	{
		return camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
	}
	public static Vector3 ViewportBL(this Camera camera)
	{
		return camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
	}

	public static float AreaOfSquare(float width, float length)
	{
		return width * length;
	}

	public static float Diag(Vector2 pointA, Vector2 pointB)
	{
		return Vector2.Distance(pointA, pointB);
	}

	public static float LongestViewportEdge (this Camera camera)
	{
		float tlToTr = Vector3.Distance(ViewportTL(camera), ViewportTR(camera));
		float tlToBL = Vector3.Distance(ViewportTL(camera), ViewportBL(camera));

		return tlToTr > tlToBL ? tlToTr : tlToBL;
	}

	public static float ShortestViewportEdge(this Camera camera)
	{
		float tlToTr = Vector3.Distance(ViewportTL(camera), ViewportTR(camera));
		float tlToBL = Vector3.Distance(ViewportTL(camera), ViewportBL(camera));

		return tlToTr > tlToBL ? tlToBL : tlToTr;
	}


	/// <summary>
	/// Rescales a Sprite Renderer to fit into a Camera's Orthographic Fustrum.
	///  Assumes the Sprite is alligned with Camera
	/// </summary>
	/// <returns></returns>
	public static void FitToOrthoCamera(this SpriteRenderer spriteRenderer, Camera camera)
	{
		spriteRenderer.transform.localScale = GetOrthoCamSize(camera) / spriteRenderer.sprite.bounds.size;
	}

	/// <summary>
	/// Returns the scale of a Sprite Renderer Sprite fit a Camera's Orthographic Fustrum
	/// Assumes the Sprite is alligned with Camera
	/// </summary>
	/// <returns></returns>
	public static Vector3 GetSpriteToOrthoCameraScale(Vector3 boundsSize, Camera camera)
	{
		return GetOrthoCamSize(camera) / boundsSize;
	}

	/// <summary>
	/// Returns a Circle's Area, given its radius
	/// </summary>
	public static float GetCircleArea(float radius) => Mathf.PI * Mathf.Pow(radius, 2);


	#region debug log
	public static bool logEnabled = true;

	/// <summary>
	/// Log a message if active
	/// </summary>
	/// <param name="str"></param>
	public static void Log(object str)
	{
		if (!logEnabled) return;

		Debug.Log(str);
	}

	public static void LogColour(object str, Color colour)
	{
		Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(colour)}>{str}</color>");
	}

	public static void LogColour(object str, string hexColour)
	{
		Debug.Log($"<color=#{hexColour}>{str}</color>");
	}

	public static string ColourText(string str, Color colour)
	{
		return $"<color=#{ColorUtility.ToHtmlStringRGB(colour)}>{str}</color>";
	}

	public static string CurrentClass
	{
		get
		{
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();

			int index = Mathf.Min(stackTrace.FrameCount - 1, 2);

			if (index < 0)
				return "{NoClass}";

			return "{" + stackTrace.GetFrame(index).GetMethod().DeclaringType.Name + "}";
		}
	}


	#endregion

	public static int LastIndex<T>(this IList<T> list) => list.Count - 1;


	public static void CopyToClipboard(this string str)
	{
		GUIUtility.systemCopyBuffer = str;
	}

	public static string FromClipboard()
	{
		return GUIUtility.systemCopyBuffer;
	}
}
