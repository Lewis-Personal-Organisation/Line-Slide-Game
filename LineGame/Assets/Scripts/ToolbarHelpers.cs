#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class ToolbarHelpers : MonoBehaviour
{
	[MenuItem("Game Editor/Save State/Reset Level")]
	static void ResetLevel()
	{
		GameSave.CurrentLevel = 0;
		Debug.Log(Utils.ColourText($"Current Level Reset to 0", Color.cyan));
	}

	[MenuItem("Game Editor/Save State/Reset Coins")]
	static void ResetCoins()
	{
		GameSave.CoinCount = 0;
		Debug.Log(Utils.ColourText($"Current Coins Reset to 0", Color.cyan));
	}

	[MenuItem("Game Editor/Save State/Reset Unlockables")]
	static void ResetUnlockables()
	{
		GameSave.ResetUnlockables();
		GameSave.Save();
		Debug.Log(Utils.ColourText($"Unlockables Reset", Color.cyan));
	}

	[MenuItem("Game Editor/Save State/Reset Level Times")]
	static void ResetLevelTimes()
	{
		GameSave.ResetLevelTimes();
		GameSave.Save();
		Debug.Log(Utils.ColourText($"Level Times Reset", Color.cyan));
	}

	[MenuItem("Game Editor/Save State/Add 200 Coins")]
	static void AddCoins()
	{
		GameSave.CoinCount += 200;
		Debug.Log(Utils.ColourText($"Added 200 Coins. New count: {GameSave.CoinCount}", Color.cyan));
	}

	[MenuItem("Game Editor/Player/Toggle Player Collisions")]
	private static void TogglePlayerCollisions()
	{
		if (GameManager.Instance)
		{
			if (GameManager.Instance.playerPathFollower != null)
			{
				bool isEnabled = GameManager.Instance.playerPathFollower.ToggleCollisions();
				Debug.Log(Utils.ColourText($"Collisions Enabled: {isEnabled}", isEnabled ? Color.green : Color.red));
			}
			else
			{
				Debug.Log(Utils.ColourText($"pathfollower of GameManager is null. Assign to use this functionality!", Color.red));
			}
			return;
		}
		else
		{
			Debug.Log(Utils.ColourText($"GameManager Instance is null. Play the Editor to uses this functionality", Color.red));
		}
	}

	[MenuItem("Game Editor/Player/Toggle Level Progression")]
	private static void ToggleLevelProgression()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.progressLevels = !GameManager.Instance.progressLevels;
		}
		else
		{
			Debug.Log(Utils.ColourText($"GameManager Instance is null. Play the Editor to uses this functionality", Color.red));
		}
	}

	[MenuItem("Game Editor/Performance/Toggle FPS Display")]
	static void ShowFPS()
	{
		if (FPSDispay.Instance)
		{
			FPSDispay.Instance.enabled = !FPSDispay.Instance.enabled;
			FPSDispay.Instance.Show();
		}
		else
		{
			Debug.Log(Utils.ColourText($"FPSDispay Instance is null. Play the Editor to uses this functionality", Color.red));
		}
	}
}
#endif