using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyCoded.HapticFeedback.Android;
using TMPro;

public class VibrationTest : MonoBehaviour
{
	[Header("UI Controls")]
	[SerializeField] private Button vibrateBtn;
	[SerializeField] private TMP_Text IntensityText;
	[SerializeField] private Button shiftLeftBtn;
	[SerializeField] private Button shiftRightBtn;
	private int vibrationIndex = 0;


	private void Awake()
	{
		IntensityText.text = $"Intensity: {vibrationIndex}";
	}

	public void Vibrate()
	{
		HapticFeedback.PerformHapticFeedback((HapticFeedbackConstants)vibrationIndex);
		Debug.Log("Vibrating");
	}

	public void ShiftIndex(int increment)
	{
		if (vibrationIndex == 0 && increment == -1)
		{
			vibrationIndex = 17;
		}
		else if (vibrationIndex == 17 && increment == 1)
		{
			vibrationIndex = 0;
		}
		else
		{
			vibrationIndex += increment;
		}

		IntensityText.text = $"Intensity: {vibrationIndex}";
	}
}
