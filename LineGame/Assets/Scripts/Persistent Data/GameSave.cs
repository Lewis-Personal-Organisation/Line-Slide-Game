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
	/// The Status of the playerSkinUnlockables
	/// </summary>
	private static int[] playerSkinUnlockables;
	private static readonly string playerSkinUnlockableString = "playerSelectionUnlockables";
	private static int activeSkin = 0;
	/// <summary>
	/// Reset the Status of all unlocks to -1 on game start. This saves us having to read from Storage each time IsPlayerSkinUnlocked(),
	/// since it's impossible to identify whether an unlock is Locked or not Initialised with 0 values
	/// Then, restore the Unlock statuses based on -1 values
	/// </summary>
	public static void ConfigureUnlocks(int count)
	{
		// Our array size should match the amount of unlockables available
		playerSkinUnlockables = new int[count];

		//for (int i = 0; i < count; i++)
		//{
		//	playerSkinUnlockables[i] = -1;
		//}

		// The player always has the first skin unlocked
		SetPlayerSkinUnlocked(0);

		// Restores the unlock state of unlockables
		for (int i = 1; i < playerSkinUnlockables.Length; i++)
		{
			RestoreUnlock(i);
		}

		// Set the previosuly Active Skin

	}

	/// <summary>
	/// Unlocks the Player Skin at Index if it is not already unlocked
	/// </summary>
	public static void SetPlayerSkinUnlocked(int index, bool applySkin = false)
	{
		if (IsPlayerSkinUnlocked(index))
		{
			if (applySkin)

				
			return;
		}
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
		//Debug.Log($"Unlockable {index} Unlocked?: {(playerSkinUnlockables[index] == 1 ? true : false)}");

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
		UITouch uITouchScript = null;

		try
		{
			uITouchScript = UnityEngine.Object.FindObjectOfType<UITouch>();
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

	//#region EXTENSIONS
	//public static bool GetBool(this PlayerPrefs prefs, string name, bool defaultChoice)
	//{
	//	return PlayerPrefs.GetInt(name, defaultChoice == true ? 1 : 0) == 1 ? true : false;
	//}
	//public static void SetBool(this PlayerPrefs prefs, string name, bool choice)
	//{
	//	PlayerPrefs.SetInt(name, choice ? 1 : 0);
	//}
	//#endregion
}