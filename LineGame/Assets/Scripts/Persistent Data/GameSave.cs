using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSave
{
	#region PREFERENCES
	public static void Save() => PlayerPrefs.Save();

	// The current game level the user has reached
	private static int currentLevel = 0;
	public static int CurrentLevel
	{
		get 
		{
			if (currentLevel == 0)
			{
				currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
			}
			return currentLevel;
		}
		set {
			// In Editor, LevelManager.instance is null - use Ternary operator to return 1 (lowest possible level)
			currentLevel = Mathf.Clamp(value, 1, LevelManager.Instance ? LevelManager.Instance.LevelCount : 1);
			PlayerPrefs.SetInt("currentLevel", currentLevel);
		}
	}

	// The current coin amount
	private static int coinCount;
	public static int CoinCount
	{
		get
		{
			if (coinCount <= 1)
				coinCount = PlayerPrefs.GetInt("coinCount", 1);
			return coinCount;
		}
		set
		{
			coinCount = value;
			PlayerPrefs.SetInt("coinCount", coinCount);
		}
	}
	#endregion
}