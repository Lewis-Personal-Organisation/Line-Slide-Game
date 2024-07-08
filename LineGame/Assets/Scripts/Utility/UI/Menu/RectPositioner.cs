using UnityEngine;

// Author: Lewis Dawson
// Description: Calculates, stores and provides a Vector3 position in percentage
// Use: When you want a RectTransform's UI element to be positioned using a percentage of a Canvas' Size
// This script ensures that UI elements will be positioned relative to the Screen Size

[RequireComponent(typeof(RectTransform))]
public class RectPositioner : MonoBehaviour
{
	internal RectTransform rectTransform = null;
	[SerializeField] internal Vector3 values;

	/// <summary>
	/// Caches the rectTransform component, when added to a RectTransform GameObject
	/// </summary>
	private void Reset()
	{
		rectTransform = GetComponent<RectTransform>();
	}

#if UNITY_EDITOR
	/// <summary>
	/// Calculates and caches the rectTransform position as Screen Resolution Percentage
	/// The Values can be used to retain the exact position of the transform on screens of any resolution
	/// </summary>
	internal void CachePosition()
	{
		if (rectTransform == null)
			rectTransform = GetComponent<RectTransform>();

		values = Utils.InverseLerp(Vector2.zero, UnityEditor.Handles.GetMainGameViewSize(), (Vector2)rectTransform.position) * 100F;
		values.z = rectTransform.position.z;
	}
#endif



	//	internal void ShowMainGameViewSize()
	//	{
	//#if UNITY_EDITOR
	//		showTextBox.text = $"{UnityEditor.Handles.GetMainGameViewSize().x}*{UnityEditor.Handles.GetMainGameViewSize().y}" +
	//						   $"{Environment.NewLine}{values}" +
	//						   $"{Environment.NewLine}{LevelManager.Instance.progressImageonscreenPos}";
	//#else
	//		showTextBox.text = $"{Screen.width}*{Screen.height}"+
	//						   $"{Environment.NewLine}{values}" +
	//						   $"{Environment.NewLine}{LevelManager.Instance.progressImageonscreenPos}";
	//#endif
	//	}

	//	private void Update()
	//	{
	//		ShowMainGameViewSize();
	//	}
}