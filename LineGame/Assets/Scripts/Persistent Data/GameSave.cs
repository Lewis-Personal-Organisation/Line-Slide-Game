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
				currentLevel = PlayerPrefs.GetInt("currentLevel", 1);
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

	private static int levelTimerEnabled = -1;
	public static bool LevelTimerEnabled
	{
		get
		{
			if (levelTimerEnabled == -1)
				levelTimerEnabled = PlayerPrefs.GetInt("timedLevels", 0);
			return (levelTimerEnabled == 1 ? true : false);
		}
		set
		{
			levelTimerEnabled = value == true ? 1 : 0;
			PlayerPrefs.SetInt("timedLevels", levelTimerEnabled);
		}
	}

	private static int[] levelTimes;
	private static readonly string levelTimerStringMS = "LevelTimeMS";
	private static readonly string levelTimerStringS = "LevelTimeS";
	/// <summary>
	/// Sets the Level Time for a given level
	/// </summary>
	public static void SetLevelTime(int levelNumber, int timeSeconds, float timeMiliseconds)
	{
		PlayerPrefs.SetInt($"{levelTimerStringS}{levelNumber}", timeSeconds);
		PlayerPrefs.SetFloat($"{levelTimerStringMS}{levelNumber}", timeMiliseconds);
	}
	/// <summary>
	/// Returns the Level Time for a given level
	/// </summary>
	public static float GetLevelTimeMS(int levelNumber)
	{
		return PlayerPrefs.GetFloat($"{levelTimerStringMS}{levelNumber}", -1F);
	}
	public static int GetLevelTimeS(int levelNumber)
	{
		return PlayerPrefs.GetInt($"{levelTimerStringS}{levelNumber}", -1);
	}

	public static float GetLevelTime(int levelNumber)
	{
		float secs = PlayerPrefs.GetInt($"{levelTimerStringS}{levelNumber}", -1);
		if (secs < 0)
			return -1;
		else
			return Mathf.Clamp(secs + PlayerPrefs.GetFloat($"{levelTimerStringMS}{levelNumber}", -1F), -1F, float.MaxValue);
	}

	public static void ResetLevelTimes()
	{
		for (int levelNumber = 0; levelNumber < LevelManager.Instance.LevelCount; levelNumber++)
		{
			PlayerPrefs.SetInt($"{levelTimerStringS}{levelNumber}", -1);
			PlayerPrefs.SetInt($"{levelTimerStringMS}{levelNumber}", -1);
		}

		Save();
	}


	#region PLAYER SKINS
	/// <summary>
	/// The Status of the Player Unlockables
	/// </summary>
	private static int[] playerSkinUnlockables;
	private static int lastAppliedSkin;
	private static readonly string playerSkinUnlockableString = "playerSelectionUnlockables";
	/// <summary>
	/// Reset the Status of all unlocks to -1 on game start. This saves us having to read from Storage each time IsPlayerSkinUnlocked(),
	/// since it's impossible to identify whether an unlock is Locked or not Initialised with 0 values
	/// Then, restore the Unlock statuses based on -1 values
	/// </summary>
	public static void ConfigureUnlocks()
	{
		// Our array size should match the amount of unlockables available
		playerSkinUnlockables = new int[UIManager.Instance.playerUnlockCount];

		// The player always has the first skin unlocked
		SetPlayerSkinUnlocked(0);

		// Restores the unlock state of unlockables
		for (int i = 1; i < playerSkinUnlockables.Length; i++)
			RestoreUnlock(i);

		// Set the default colours active, if we have not unlocked anything
		// Else, if we have unlocked something, we need to check the last applied skin
		if (UnlocksAreDefault())
			UIManager.Instance.SwapPlayerColoursToDefault();
		else
			UIManager.Instance.ApplyLastUnlockedSkin(GetLastSkinIndex());
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
	/// Sets the last applied skin to PlayerPrefs
	/// </summary>
	public static void SetLastSkinIndex(int index)
	{
		lastAppliedSkin = index;
		PlayerPrefs.SetInt("lastSkinSelected", lastAppliedSkin);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// Returns the last applied skin for the Player. If not skin has been set, set it to 0 (default skin)
	/// </summary>
	public static int GetLastSkinIndex()
	{
		lastAppliedSkin = PlayerPrefs.GetInt("lastSkinSelected", 0);
		return lastAppliedSkin;
	}


	/// <summary>
	/// Restore the active state of an unlockable. UI elements are updated based on the playerSkinUnlockables array
	/// </summary>
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
			for (int i = 1; i < UIManager.Instance.playerUnlockCount; i++)
			{
				PlayerPrefs.SetInt($"{playerSkinUnlockableString}{i}", -1);
			}
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	/// <summary>
	/// Checks if the Unlocks are in the default state - Used to avoid exploit: User data deletion, but last applied skin remains active
	/// </summary>
	private static bool UnlocksAreDefault()
	{
		for (int i = 1; i < UIManager.Instance.playerUnlockCount; i++)
			if (PlayerPrefs.GetInt($"{playerSkinUnlockableString}{i}") != -1)
				return false;

		return true;
	}
	#endregion
	#endregion
}