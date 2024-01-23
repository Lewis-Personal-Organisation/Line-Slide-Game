using UnityEngine;
using TMPro;
using UnityEditor;
using System;

[RequireComponent(typeof(RectTransform))]
public class RectPositioner : MonoBehaviour
{
	[SerializeField] internal RectTransform rectTransform = null;
	[SerializeField] internal Vector3 values;
	[SerializeField] internal TextMeshProUGUI showTextBox;

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