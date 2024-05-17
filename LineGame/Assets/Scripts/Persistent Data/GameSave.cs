using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
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
	private static int coinCount = -1;
	public static int CoinCount
	{
		get
		{
			if (coinCount == -1)
				coinCount = PlayerPrefs.GetInt("coinCount", 0);
			return coinCount;
		}
		set
		{
			coinCount = value;
			PlayerPrefs.SetInt("coinCount", coinCount);
		}
	}

	/// <summary>
	/// The Status of the Player Unlockables
	/// </summary>
	private static int[] playerSkinUnlockables;
	private static readonly string playerSkinUnlockableString = "playerSelectionUnlockables";
	/// <summary>
	/// Reset the Status of all unlocks to -1 on game start. This saves us having to read from Storage each time IsPlayerSkinUnlocked(),
	/// since it's impossible to identify whether an unlock is Locked or not Initialised with 0 values
	/// Then, restore the Unlock statuses based on -1 values
	/// </summary>
	public static void ConfigureUnlocks()
	{
		// Our array size should match the amount of unlockables available
		playerSkinUnlockables = new int[UITouch.Instance.playerUnlockCount];

		// The player always has the first skin unlocked
		SetPlayerSkinUnlocked(0);

		// Restores the unlock state of unlockables
		for (int i = 1; i < playerSkinUnlockables.Length; i++)
		{
			RestoreUnlock(i);
		}
	}

	/// <summary>
	/// Unlocks the Player Skin at Index if it is not already unlocked
	/// </summary>
	public static void SetPlayerSkinUnlocked(int index, bool applySkin = false)
	{
		if (IsPlayerSkinUnlocked(index))
			return;

		index = Utils.Clamp(index, 0, playerSkinUnlockables.Length-1);
		PlayerPrefs.SetInt($"{playerSkinUnlockableString}{index}", 1);
		playerSkinUnlockables[index] = 1;
	}

	/// <summary>
	/// Gets the unlock status of the Player Skin at index
	/// </summary>
	public static bool IsPlayerSkinUnlocked(int index)
	{
		index = Utils.Clamp(index, 0, playerSkinUnlockables.Length - 1);
		return playerSkinUnlockables[index] == 1 ? true : false;
	}

	/// <summary>
	/// Restore the active state of an unlockable. UI elements are updated based on the playerSkinUnlockables array
	/// </summary>
	/// <param name="index"></param>
	private static void RestoreUnlock(int index)
	{
		playerSkinUnlockables[index] = PlayerPrefs.GetInt($"{playerSkinUnlockableString}{index}", -1);
	}

	/// <summary>
	/// Force all unlockables to be locked
	/// </summary>
	public static void ResetUnlockables()
	{
		try
		{
			UITouch uITouchScript = UnityEngine.Object.FindObjectOfType<UITouch>();
			for (int i = 1; i < uITouchScript.playerUnlockCount; i++)
			{
				PlayerPrefs.SetInt($"{playerSkinUnlockableString}{i}", -1);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}
	#endregion
}