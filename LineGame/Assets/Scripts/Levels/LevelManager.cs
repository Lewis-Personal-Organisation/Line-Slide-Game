using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class LevelManager : Singleton<LevelManager>
{
	private GameObject levelObject;
	public Level currentLevel;

	[Header("Game Levels")]
	public List<Level> levels = new List<Level>();
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

	internal float particleSystemOneDistance;
	internal float particleSystemTwoDistance;
	internal float particleSystemThreeDistance;

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

	private void Start()
	{
		progressImageOffscreenPos = CanvasUtils.GetPos(progressImage.rectTransform, CanvasPositions.Top, UITouch.Instance.gameplayCanvas.scaleFactor, UITouch.Instance.gameplayCanvasTransform);
		progressImageonscreenPos = UITouch.Instance.gameplayCanvasTransform.GetPos(UITouch.Instance.imagePositioner.values);
		progressImage.rectTransform.position = progressImageOffscreenPos;

		//Debug.Log($"OFF POS: {progressImageOffscreenPos} | ON POS: {progressImageonscreenPos} | ANCHORS: Min - {UITouch.Instance.imagePositioner.rectTransform.anchorMin}, Max - {UITouch.Instance.imagePositioner.rectTransform.anchorMax}");
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
		levelObject = Instantiate(GetLevelObject(levelNum), null);
		levelObject.transform.position = Vector3.zero;
		currentLevel = levelObject.GetComponent<Level>();
		particleSystemOneDistance = currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.particles[0].transform.position);
		particleSystemTwoDistance = currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.particles[2].transform.position);
		particleSystemThreeDistance = currentLevel.roadPathCreator.path.GetClosestDistanceAlongPath(currentLevel.particles[4].transform.position);
		UITouch.Instance.progressText.text = $"Level {levelNum}";
		WaterShaderAnimator.Instance.SetMeshRenderer(currentLevel.WaterMesh);
		GameManager.Instance.playerPathFollower.Setup();

		OnLevelComplete = delegate ()
		{
			StartCoroutine(LevelComplete());
		};

		UITouch.Instance.SwitchView(UITouch.ViewStates.LoadLevel);
	}

	/// <summary>
	/// Called when we reach finish line. Increments the Level preference and fades the level mask
	/// </summary>
	public IEnumerator LevelComplete()
	{
		OnLevelComplete = null;
		GameSave.CurrentLevel++;
		GameSave.Save();

		GameManager.Instance.playerPathFollower.ApplyCruiseSpeed();
		Coroutine tempShake = StartCoroutine(ShakeTreasureChest()); // Begin shaking chest when we cross finish line

		// When we have done 50% of the Finish Line to End section of path, Fire confetti
		//yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.finishToEndPercent > 50F);

		yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.PSOneReached);
		FireConfetti(ConfettiSets.One);

		yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.PSTwoReached);
		FireConfetti(ConfettiSets.Two);

		yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.PSThreeReached);
		FireConfetti(ConfettiSets.Three);


		// Wait until we reach the end of path
		yield return new WaitUntil(() => GameManager.Instance.playerPathFollower.pathComplete);

		// Stop chest shaking and open
		StopCoroutine(tempShake);

		yield return StartCoroutine(OpenTreasureChest(new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Single, .8F, FireTreasureChestCoins)));

		// Trigger and Move Player through finish zone (Fire confetti), 
		// When they stop, Fire chest with Coins

		//UITouch.Instance.SwitchView(UITouch.ViewStates.LevelComplete);
		//yield return new WaitUntil(() => UITouch.Instance.maskScaleState == ScaleStates.Inactive);
		//Debug.Log(Utils.ColourText($"Fade has finished! Loading new level", Color.cyan));
		//LoadLevel(GameSave.CurrentLevel);
		yield return null;
	}

	/// <summary>
	/// Update our level progress image to match our travelled distance in the level
	/// </summary>
	/// <param name="distance"></param>
	/// <param name="vertCount"></param>
	public void UpdateUIProgress(float distance, float vertCount)
	{
		progressImage.fillAmount = distance / vertCount;
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

	private IEnumerator OpenTreasureChest(CanvasUtils.TimedAction timedAction)
	{
		float t = 0;
		while ((t += Time.deltaTime) < 1)
		{
			currentLevel.treasureChestPivot.localEulerAngles = Vector3.right * 100F * GameManager.Instance.curveHelper.Evaluate(CurveType.Exponential, CurveMode.Out, t);
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
		Debug.Log(Utils.ColourText("COIN FIRE TEST", Color.yellow));
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
				currentLevel.particles[0].Play();
				currentLevel.particles[0 + 1].Play();
				break;
			case ConfettiSets.Two:
				currentLevel.particles[2].Play();
				currentLevel.particles[2 + 1].Play();
				break;
			case ConfettiSets.Three:
				currentLevel.particles[4].Play();
				currentLevel.particles[4 + 1].Play();
				break;
		}
	}
}