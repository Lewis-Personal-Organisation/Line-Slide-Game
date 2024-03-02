using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class RectPositioner : MonoBehaviour
{
	[SerializeField] internal RectTransform rectTransform = null;
	[SerializeField] internal Vector3 values;
	[SerializeField] internal TextMeshProUGUI showTextBox;

	/// <summary>
	/// Caches the position of the rectTransform using the current Game WidthxHeight of the Game View as a Percentage
	/// The Values can be reused to retain the exact position of the transform on screens of any resolution
	/// </summary>
	internal void CachePosition()
	{
		values = Utils.InverseLerp(Vector2.zero, UnityEditor.Handles.GetMainGameViewSize(), (Vector2)rectTransform.position) * 100F;
		values.z = rectTransform.position.z;
	}

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