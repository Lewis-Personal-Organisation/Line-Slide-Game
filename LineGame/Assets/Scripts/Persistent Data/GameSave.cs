using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSave : MonoBehaviour
{
	#region PREFERENCES
	private static int currentLevel;
	// The current game level the user has reached
	public static int CurrentLevel
	{
		get 
		{
			if (currentLevel <= 0)
				currentLevel = PlayerPrefs.GetInt("currentLevel", 1);
			return currentLevel;
		}
		set { 
			currentLevel = Mathf.Clamp(value, 0, GameManager.levelCount);
			PlayerPrefs.SetInt("currentLevel", currentLevel);
			PlayerPrefs.Save();
		}
	}
	#endregion
}
