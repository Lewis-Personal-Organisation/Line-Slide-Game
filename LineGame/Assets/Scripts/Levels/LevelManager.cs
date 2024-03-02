using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using static Level;
using System.Runtime.InteropServices.WindowsRuntime;

public class LevelManager : Singleton<LevelManager>
{
	private GameObject levelObject;
	public Level currentLevel;

	[Header("Game Levels")]
	public List<Level> levels = new List<Level>();
	public Level testLevel;
	public int LevelCount => levels.Count;
	public static UnityAction OnLevelComplete;

	[Header("Progress Image")]
	public SlicedFilledImage progressImage;
	public Vector2 progressImageonscreenPos;
	public Vector2 progressImageOffscreenPos;


	[Header("Player Trail Colours")]
	[Space(10)]
	public Color beginnerTrailColour;
	public Color IntermediateTrailColour;
	public Color hardTrailColour;
	public Color impossibleTrailColour;

	[Header("Player Water Colours")]
	[Space(10)]
	public Color beginnerWaterColour;
	public Color IntermediateWaterColour;
	public Color hardWaterColour;
	public Color impossibleWaterColour;

	[Header("Treasure Chest Shake Variables")]
	public float forward = 0;
	public float side = 0;
	public float forwardSpeed = 0;
	public float sideSpeed = 0;

	[Header("Coin Velocity")]
	private bool clampCoinVelocity = false;

	private bool PlayerHasPassedFirstParticleSystem => currentLevel.pathCreator.path.GetClosestDistanceAlongPath(GameManager.Instance.playerPathFollower.transform.position) >= currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.finishingParticleSystems[0].transform.position);
	private bool PlayerHasPassedSecondParticleSystem => currentLevel.pathCreator.path.GetClosestDistanceAlongPath(GameManager.Instance.playerPathFollower.transform.position) >= currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.finishingParticleSystems[2].transform.position);
	private bool PlayerHasPassedThirdParticleSystem => currentLevel.pathCreator.path.GetClosestDistanceAlongPath(GameManager.Instance.playerPathFollower.transform.position) >= currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.finishingParticleSystems[4].transform.position);

	public enum ConfettiSets
	{
		One,
		Two,
		Three
	}


	new private void Awake()
	{
		base.Awake();
	}

	private void FixedUpdate()
	{
		if (!clampCoinVelocity)
			return;

		foreach (var coin in currentLevel.TreasureChestCoins)
		{
			coin.velocity = Vector3.ClampMagnitude(coin.velocity, 10F);
		}
	}

	/// <summary>
	/// Loads a level given a level number. Sets the level objects position to 0,0,0 and Assign level variables for player
	/// Set's the Level number, Water Mesh colour and subscribes to the path
	/// Note: Level number is auto-adjusted to 0-based index
	/// </summary>
	public void LoadLevel(int levelNum)
	{
		// Destroy old Level
		Destroy(levelObject);

		// Create new level
		levelObject = Instantiate( levelNum == -1 ? testLevel.gameObject : GetLevelObject(levelNum), null);
		levelObject.transform.position = Vector3.zero;
		currentLevel = levelObject.GetComponent<Level>();
		SetupLevel();

		UITouch.Instance.progressText.text = levelNum == -1 ? "Test Level" : $"Level {levelNum}";
		WaterShaderAnimator.Instance.SetMeshRenderer(currentLevel.WaterMesh);
		GameManager.Instance.playerPathFollower.Setup();
		clampCoinVelocity = false;

		OnLevelComplete = delegate ()
		{
			StartCoroutine(LevelComplete());
		};

		UITouch.Instance.SwitchView(UITouch.ViewStates.LoadLevel);
	}

	/// <summary>
	/// Sets up various level objects
	/// </summary>
	private void SetupLevel()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying == false)
			return;
#endif
		switch (currentLevel.Difficulty)
		{
			case LevelDifficulty.Beginner:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = beginnerTrailColour;
				GameManager.Instance.playerPathFollower.playerMaterial.color = beginnerTrailColour;
				GameManager.Instance.playerParticleMaterial.color = GetOffsetColour(beginnerTrailColour, -100F);
				progressImage.color = beginnerTrailColour;
				currentLevel.WaterMesh.material.SetColor("_WaterColor", beginnerWaterColour);
				break;
			case LevelDifficulty.Intermediate:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = IntermediateTrailColour;
				GameManager.Instance.playerPathFollower.playerMaterial.color = IntermediateTrailColour;
				GameManager.Instance.playerParticleMaterial.color = GetOffsetColour(beginnerTrailColour, -100F);
				progressImage.color = IntermediateTrailColour;
				currentLevel.WaterMesh.material.SetColor("_WaterColor", IntermediateWaterColour);
				break;
			case LevelDifficulty.Hard:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = hardTrailColour;
				GameManager.Instance.playerPathFollower.playerMaterial.color = hardTrailColour;
				GameManager.Instance.playerParticleMaterial.color = GetOffsetColour(beginnerTrailColour, -100F);
				progressImage.color = hardTrailColour;
				currentLevel.WaterMesh.material.SetColor("_WaterColor", hardWaterColour);
				break;
			case LevelDifficulty.Impossible:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = impossibleTrailColour;
				GameManager.Instance.playerPathFollower.playerMaterial.color = impossibleTrailColour;
				GameManager.Instance.playerParticleMaterial.color = GetOffsetColour(beginnerTrailColour, -100F);
				progressImage.color = impossibleTrailColour;
				currentLevel.WaterMesh.material.SetColor("_WaterColor", impossibleWaterColour);
				break;
		}
	}

	/// <summary>
	/// Retuns an offset colour of an original. Offset values -> 0 - 255
	/// </summary>
	/// <param name="original"></param>
	/// <param name="offset"></param>
	/// <returns></returns>
	public Color GetOffsetColour(Color original, float offset)
	{
		float amount = offset * (1F / 255F);
		return new Color(original.r + amount, original.g + amount, original.b + amount, original.a);
	}
	/// <summary>
	/// Called when we reach finish line - Fires Confetti, Shakes and Opens Chest, Fires Coins, Processes UI Fades
	/// /// </summary>
	public IEnumerator LevelComplete()
	{
		OnLevelComplete = null;
		GameSave.CurrentLevel++;
		GameSave.Save();

		GameManager.Instance.playerPathFollower.ApplyCruiseSpeed();
		Coroutine ChestShakeRoutine = StartCoroutine(ShakeTreasureChest()); // Begin shaking chest when we cross finish line

		// Wait for the Player to pass each Particle system before playihng confety particle systems
		yield return new WaitUntil(() => PlayerHasPassedFirstParticleSystem);
		FireConfetti(ConfettiSets.One);

		yield return new WaitUntil(() => PlayerHasPassedSecondParticleSystem);
		FireConfetti(ConfettiSets.Two);

		yield return new WaitUntil(() => PlayerHasPassedThirdParticleSystem);
		FireConfetti(ConfettiSets.Three);

		// Wait until we reach the end of path
		yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.pathComplete);

		// Stop emitting particles
		GameManager.Instance.playerPathFollower.playerParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);

		// Stop chest shaking and open
		StopCoroutine(ChestShakeRoutine);

		// Open the treasure chest and fire coins
		yield return StartCoroutine(OpenTreasureChest(new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Passive, .5F, FireTreasureChestCoins)));

		// Wait until coins have settled in place
		yield return new WaitForSeconds(3);

		// Water Animations for coins and cubes
		// Change Level complete screen
		yield return StartCoroutine(MoveCoinsToCoinCounter());

		yield return new WaitForSeconds(1.5F);

		UITouch.Instance.SwitchView(UITouch.ViewStates.LevelComplete);
		yield return new WaitUntil(() => UITouch.Instance.maskScaleState == ScaleStates.Inactive);
		Debug.Log(Utils.ColourText($"Fade has finished! Loading new level", Color.cyan));
		LoadLevel(GameSave.CurrentLevel);
		yield return null;
	}

	/// <summary>
	/// Get the level. Call this method with the minimum level being 1
	/// </summary>
	/// <param name="level"></param>
	/// <returns></returns>
	private GameObject GetLevelObject(int level)
	{
		return levels[Mathf.Clamp(level - 1, 0, levels.Count)].gameObject;
	}

	// Open the chest over a period of 1 second
	private IEnumerator OpenTreasureChest(CanvasUtils.TimedAction timedAction)
	{
		float t = Time.time;
		while (Time.time - t < 1)
		{
			currentLevel.treasureChestPivot.localEulerAngles = Vector3.right * 100F * GameManager.Instance.curveHelper.Evaluate(CurveType.Exponential, CurveMode.Out, Time.time - t);
			timedAction.Evaluate(t);
			yield return null;
		}
	}

	public IEnumerator ShakeTreasureChest()
	{
		Vector3 cachedAngles = currentLevel.treasureChestPivot.parent.localEulerAngles;

		while (true)
		{
			Vector3 shake = new Vector3(Mathf.Lerp(forward, -forward, Mathf.PingPong(Time.time * forwardSpeed, 1)), 0, Mathf.Lerp(-side, side, Mathf.PingPong(Time.time * sideSpeed, 1)));
			currentLevel.treasureChestPivot.parent.localEulerAngles = cachedAngles + shake;
			yield return null;
		}
	}

	private void FireTreasureChestCoins()
	{
		clampCoinVelocity = true;

		foreach (var coinRB in currentLevel.TreasureChestCoins)
		{
			coinRB.AddTorque(new Vector3(Random.Range(2, 50), Random.Range(2, 50), Random.Range(2, 50)));
			coinRB.AddForce(Vector3.up * 450F, ForceMode.Force);
		}
	}

	/// <summary>
	/// Fire the Confetti Particle Systems using the set
	/// </summary>
	/// <param name="x"></param>
	private void FireConfetti(ConfettiSets set)
	{
		switch (set)
		{
			case ConfettiSets.One:
				currentLevel.finishingParticleSystems[0].Play();
				currentLevel.finishingParticleSystems[0 + 1].Play();
				break;
			case ConfettiSets.Two:
				currentLevel.finishingParticleSystems[2].Play();
				currentLevel.finishingParticleSystems[2 + 1].Play();
				break;
			case ConfettiSets.Three:
				currentLevel.finishingParticleSystems[4].Play();
				currentLevel.finishingParticleSystems[4 + 1].Play();
				break;
		}
	}

	/// <summary>
	/// Spawn the UI Coins in the world to screen position of where they landed
	/// If they land offscreen, spawn them at a random radius from screen center
	/// </summary>
	public IEnumerator MoveCoinsToCoinCounter()
	{
		List<Vector2> startPositions = new List<Vector2>();

		for (int i = 0; i < currentLevel.TreasureChestCoins.Length; i++)
		{
			UITouch.Instance.coinImages[i].transform.position = GameManager.Instance.mainCamera.WorldToScreenPoint(currentLevel.TreasureChestCoins[i].transform.position);
			UITouch.Instance.coinImages[i].gameObject.SetActive(true);
			startPositions.Add(UITouch.Instance.coinImages[i].rectTransform.position);
		}

		float t = 0;

		while(t < 1)
		{
			for (int i = 0; i < UITouch.Instance.coinImages.Count; i++)
			{
				UITouch.Instance.coinImages[i].rectTransform.position = Vector3.Slerp(startPositions[i], UITouch.Instance.coinCounterImage.rectTransform.position, t);
				UITouch.Instance.coinImages[i].rectTransform.localScale = Vector2.one * UITouch.Instance.coinSizeCurve.Evaluate(t);
			}

			t += Time.deltaTime;
			yield return null;
		}

		for (int i = 0; i < UITouch.Instance.coinImages.Count; i++)
		{
			UITouch.Instance.coinImages[i].gameObject.SetActive(false);
		}

		float coinCountToUpdate = GameSave.CoinCount;		// The Coin Count to update
		GameSave.CoinCount += ProcessLevelReward();
		GameSave.Save();

		// While coinCountToUpdate does not match the new coin count, update it visually
		while (coinCountToUpdate < GameSave.CoinCount)
		{
			coinCountToUpdate += Time.deltaTime * UITouch.Instance.coinCounterUpdateSpeed;
			UITouch.Instance.coinCounterText.text = ((int)coinCountToUpdate).ToString();
			yield return null;
		}
	}

	private int ProcessLevelReward()
	{
		switch (currentLevel.Difficulty)
		{
			case LevelDifficulty.Beginner:
				return 50;
			case LevelDifficulty.Intermediate:
				return 75;
			case LevelDifficulty.Hard:
				return 110;
			case LevelDifficulty.Impossible:
				return 160;
		}
		return 0;
	}

	
}