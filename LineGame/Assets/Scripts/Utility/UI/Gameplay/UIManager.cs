// Author: Lewis Dawson
// Title: UITouch Graphics Raycast Manager
// Description: Processes UI touch functionality using graphics raycasting for specified objects using their InstanceID()
// Object IDs are linked in Awake() to a Touch Filter. Filters functionality is then processed on object touch
// Object can choose to report as a touch or non-touch. For example, the "Tap-to-play" object should report as non-touch, so
// we can continue to touch the screen for movement
// Limitations: Currently only work with 1 Canvas. Filters are fixed - can't be added at runtime

// RENAME TO UI MANAGER

using RDG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	public bool touchingOverFrames = false;		// Used to filter out touch spamming by holding down a touch
	public bool isTouchingUIElement = false;	// Determines if we are touching a UI item

	public enum Fade
	{
		ToTransparent,
		ToOpaque,
	}
	public enum Scale
	{
		Up,
		Down,
	}
	public enum ScaleStates
	{
		Inactive,
		InProgress,
	}
	private Dictionary<int, Action> InstanceIDtoAction = new Dictionary<int, Action>();

	// Graphics raycasting
	[Header("Raycasting Systems")]
    [SerializeField] private GraphicRaycaster gameplayUIRaycaster = null;
	[SerializeField] private GraphicRaycaster levelFailedUIRaycaster = null;
	[SerializeField] private GraphicRaycaster playerSelectUIRaycaster = null;
	[SerializeField] private PointerEventData uiPointerEventData = null;
    [SerializeField] private EventSystem uiEventSystem = null;

	[Header("UI Elements")]
	public Canvas gameplayCanvas;
	public RectTransform gameplayCanvasTransform;
    public CanvasGroup gameplayCanvasGroup;
	public CanvasGroup levelFailedCanvasGroup;
	[SerializeField] TextMeshProUGUI timerMilisecondsText;
	[SerializeField] TextMeshProUGUI timerSecondsText;
	[SerializeField] int timerSecondsValue;
	[SerializeField] private float timerMilisecondsValue;
	private bool updateTimerValue;


	[SerializeField] TMP_Text levelPercentTMPText = null;
	public string levelPercentText
	{
		set { levelPercentTMPText.text = value; }
	}
	public TextMeshProUGUI progressText;
	private Vector2 progressImageOffScreenPos = Vector2.zero;
	private Vector2 progressImageOnScreenPos = Vector2.zero;
	public SlicedFilledImage levelProgressImage = null;
	private Vector3 maskFadeSpriteMaxSize = Vector3.zero;
	private ScaleStates maskScaleState;
	public bool maskScaleActive => maskScaleState == ScaleStates.InProgress;

	[Header("Coins")]
	public Image coinCounterImage;
	public float coinCounterUpdateSpeed = 1;
	public TextMeshProUGUI coinCounterText;
	public AnimationCurve coinSizeCurve;
	public List<Image> coinImages;

	public enum ViewStates
	{
		LevelLoaded,
		LevelComplete,
		LevelFailed,
		LevelRestart,
		PlayerSelection,
	}
	private static ViewStates viewState;
	private static ViewStates previousViewState;


	[Header("Gameplay View Interactables")]
	public Settings settings;
	[System.Serializable]
	public struct Settings
	{
		[Header("Main")]
		public Image Button;
		public CanvasGroup canvasGroup;
		public GameObject slidingObject;
		public Vector2 backBaseSize;
		public Vector2 backExtSize;
		public Image backgroundImage;
		[SerializeField] public bool usable;
		public bool open;
		public bool isAnimating;
		public Color onColour;
		public Color offColour;

		[Header("Vibration")]
		public Image vibrateImage;
		public Image vibrateBackgroundImage;

		[Header("Level Timing")]
		public Image timerImage;
		public Image timerBackgroundImage;

		public void Toggle(bool choice)
		{
			Button.raycastTarget = choice;
			vibrateImage.raycastTarget = choice;
			timerImage.raycastTarget = choice;
		}
	}
	[Space(10)]
	[SerializeField] TextMeshProUGUI vSyncText = null;
	public TextBounce tapToPlay;
    public RectTransform tapToPlayHitBox;
	public RectTransform playerSelectionHitBox;
	public RectTransform PlayerSelectionReturnHitBox;

	[Header("Restart View Interactables")]
	public TextBounce tapToRestart;
	public RectTransform tapToRestartHitBox;
	private bool tapToRestartEnabled = false;
    [SerializeField] private SpriteRenderer spriteRenderer = null;
	[SerializeField] private Transform maskFadeSprite;
	[SerializeField] private Color levelFailedColour = Color.clear;
	public RectPositioner imagePositioner;

	[Header("Player Selection Variables")]
	public CanvasGroup playerSelectCanvasGroup = null;
	[SerializeField] private float backgroundImageScrollSpeed;
	[SerializeField] private Material playerSelectScrollingBackgroundImage;
	[SerializeField] private Image playerSelectbackgroundImage;
	[SerializeField] private Material playerSelectHighlightMat;
	[SerializeField] private Material playerSelectLockedMat;
	[SerializeField] private Color playerSelectLockedColourCached;
	[SerializeField] private MeshRenderer previewCubeMeshRenderer;
	[SerializeField] private Material previewCubeMaterial;
	public Rotate previewCubeRotator;
	[SerializeField] private GameObject playerSelectionPurchaseButton;
	[SerializeField] private TextMeshProUGUI playerSelectionPurchaseText;
	[SerializeField] private GameObject playerSelectionPurchaseButtonOverlay;
	[SerializeField] List<PlayerUnlockable> playerUnlockables = new List<PlayerUnlockable>();
	public int playerUnlockCount => playerUnlockables.Count;
	private int skinIndex = -1;
	private Coroutine playerSelectScrollCoroutine;

	public void EnableLevelTimer(bool choice) => updateTimerValue = choice;
	public float LevelTimerMilisecondsValue() => timerMilisecondsValue;
	public int LevelTimerSecondsValue() => timerSecondsValue;
	public float LevelTimerValue() => timerSecondsValue + timerMilisecondsValue;


	new private void Awake()
	{
		base.Awake();

		LinkUIIDActions();

		FPSDispay.Instance.enabled = false;
		vSyncText.gameObject.SetActive(false);

		maskFadeSpriteMaxSize = maskFadeSprite.transform.localScale;
		gameplayCanvasGroup.alpha = 0;

		// Cache the positions for our Progress Image
		progressImageOffScreenPos = CanvasUtils.GetPos(levelProgressImage.rectTransform, CanvasPositions.Top, Instance.gameplayCanvas.scaleFactor, Instance.gameplayCanvasTransform);
		progressImageOnScreenPos = Instance.gameplayCanvasTransform.GetPos(Instance.imagePositioner.values);
		levelProgressImage.rectTransform.position = progressImageOffScreenPos;

		playerUnlockables[0].selectableImage.material.color = new Color(playerUnlockables[0].selectableImage.material.color.r,
																		playerUnlockables[0].selectableImage.material.color.g,
																		playerUnlockables[0].selectableImage.material.color.b,
																		1F);

		ResetPlayerSelectionCubes();
	}

	// Each frame, if we have provided Mouse/Touch input, and aren't touching over multiple frames,
	// Graphics Raycast our screen with the touch position. If we touch an object with a mathcing InstanceID, process its filter
	// Store all hit objects hit, in our raycast list
	private void Update()
    {
		if (Input.GetKey(KeyCode.Mouse0) || Input.touchCount > 0)
		{
			// If we touched the screen over multiple frames, don't accept input
			if (!touchingOverFrames)
			{
				// If we didn't touch screen in last update, We are now touching over multiple frames with this frame
				touchingOverFrames = true;

				// Set up the new Pointer Event and assign the mouse position
				uiPointerEventData = new PointerEventData(uiEventSystem);
				uiPointerEventData.position = Input.mousePosition;

				// Create a new list of results for our raycast
				List<RaycastResult> hitResults = new List<RaycastResult>();

				// Raycast using the Graphics Raycaster and mouse click position dependant on view state
				if (viewState == ViewStates.LevelFailed)
					levelFailedUIRaycaster.Raycast(uiPointerEventData, hitResults);
				else if (viewState == ViewStates.PlayerSelection)
					playerSelectUIRaycaster.Raycast(uiPointerEventData, hitResults);
				else
					gameplayUIRaycaster.Raycast(uiPointerEventData, hitResults);

				for (int i = 0; i < hitResults.Count; i++)
				{
					// Does this ID have an action?
					if (InstanceIDtoAction.ContainsKey(hitResults[i].gameObject.transform.GetInstanceID()))
					{
						//Debug.Log(Utils.ColourText($"Touched {hitResults[i].gameObject.transform.name}", Color.green));
						InstanceIDtoAction[hitResults[i].gameObject.transform.GetInstanceID()].Invoke();

						if (isTouchingUIElement)
							break;
					}
					else
					{
						Debug.Log(Utils.ColourText($"UITouch.cs :: ID for {hitResults[i].gameObject.transform.GetInstanceID()} - {hitResults[i].gameObject.name} has no action", Color.cyan));
					}
				}
			}
		}
		else
		{
			touchingOverFrames = false;
			isTouchingUIElement = false;
		}

		if (GameSave.LevelTimerEnabled && updateTimerValue)
		{
			timerMilisecondsValue += (Time.deltaTime);
			if (timerMilisecondsValue > 1)
			{
				timerMilisecondsValue -= 1F;
				timerSecondsValue += 1;
			}

			timerSecondsText.text = timerSecondsValue.ToString();
			timerMilisecondsText.text = timerMilisecondsValue.ToString(".##");
		}
    }

	/// <summary>
	/// Assigns the touchable/interactable Objects to custom functionality.
	/// InstanceID's have to be set using the Transform Component of an Object
	/// </summary>
	private void LinkUIIDActions()
	{
		InstanceIDtoAction.Add(settings.Button.transform.GetInstanceID(), () =>
		{
			if (!settings.isAnimating)
			{
				StartCoroutine(AnimateSettings(!settings.open));
				settings.isAnimating = true;
				isTouchingUIElement = true;
			}
		});
		InstanceIDtoAction.Add(settings.vibrateImage.transform.GetInstanceID(), () =>
		{
			Vibration.enabled = !Vibration.enabled;
			PlayerPrefs.SetInt("vibrationState", Vibration.enabled ? 1 : 0);
			PlayerPrefs.Save();
			settings.vibrateBackgroundImage.color = Vibration.enabled ? settings.onColour : settings.offColour;
			
			if (Vibration.enabled)
				Vibration.Vibrate(100);

			isTouchingUIElement = true;
		});
		InstanceIDtoAction.Add(settings.timerImage.transform.GetInstanceID(), () =>
		{
			GameSave.LevelTimerEnabled = !GameSave.LevelTimerEnabled;
			GameSave.Save();

			if (GameSave.LevelTimerEnabled)
			{
				timerSecondsText.text = Mathf.Clamp(GameSave.GetLevelTimeS(GameSave.CurrentLevel), 0, int.MaxValue).ToString();
				timerSecondsText.gameObject.SetActive(true);
				timerMilisecondsText.text = Mathf.Clamp(GameSave.GetLevelTimeMS(GameSave.CurrentLevel), 0, float.MaxValue).ToString(".##");
				timerMilisecondsText.gameObject.SetActive(true);
			}
			else
			{
				timerSecondsText.gameObject.SetActive(false);
				timerMilisecondsText.gameObject.SetActive(false);
			}

			settings.timerBackgroundImage.color = GameSave.LevelTimerEnabled ? settings.onColour : settings.offColour;
			isTouchingUIElement = true;
		});
		InstanceIDtoAction.Add(tapToPlayHitBox.GetInstanceID(), () => {

			if (GameSave.LevelTimerEnabled)
			{
				ResetLevelTimer();
				updateTimerValue = true;
			}

			settings.Toggle(false);

			if (settings.open)
			{
				StartCoroutine(AnimateSettings(false));
				settings.isAnimating = true;
			}

			FadeCanvasGroup(Fade.ToTransparent, settings.canvasGroup, 2.9F);

			tapToPlay.gameObject.SetActive(false);
			tapToPlayHitBox.gameObject.SetActive(false);
			Instance.levelProgressImage.rectTransform.Move(this, Instance.progressImageOffScreenPos, Instance.progressImageOnScreenPos, 1.2F, CurveType.Exponential);
			playerSelectionHitBox.gameObject.SetActive(false);
			GameManager.Instance.playerPathFollower.SetTrailDistance();
			GameManager.Instance.playerPathFollower.SetPlayerControl(true);
			isTouchingUIElement = false;
		});
		InstanceIDtoAction.Add(tapToRestartHitBox.GetInstanceID(), () => {
			if (!tapToRestartEnabled)
				return;
			SwitchView(ViewStates.LevelRestart);
			isTouchingUIElement = true;
		});
		InstanceIDtoAction.Add(playerSelectionHitBox.GetInstanceID(), () => {
			SwitchView(ViewStates.PlayerSelection);
			isTouchingUIElement = true;
		});
		InstanceIDtoAction.Add(playerSelectionPurchaseButton.transform.GetInstanceID(), () => {
			if (GameSave.CoinCount < playerUnlockables[skinIndex].cost)
				return;
			GameSave.SetPlayerSkinUnlocked(skinIndex, true);
			playerSelectionPurchaseButton.SetActive(false);
			playerUnlockables[skinIndex].overlay.gameObject.SetActive(false);
			float oldCoinCount = GameSave.CoinCount;
			GameSave.CoinCount -= playerUnlockables[skinIndex].cost;
			GameSave.Save();
			UpdateUICoins(oldCoinCount, 240);
			SwapPlayerColoursOnSelection();
			isTouchingUIElement = true;
		});
		InstanceIDtoAction.Add(PlayerSelectionReturnHitBox.GetInstanceID(), () => {

			Vector2 cachedSize = PlayerSelectionReturnHitBox.sizeDelta;  // Cache the og size

			// Scale down, and scale up once complete
			PlayerSelectionReturnHitBox.ResizeOverTime(this, cachedSize * CanvasUtils.Negative2D * 0.20F, .075F, null, () =>
			{
				PlayerSelectionReturnHitBox.ResizeOverTime(this, cachedSize * CanvasUtils.Positive2D * 0.20F, .075F);
			});

			SwitchView(ViewStates.LevelLoaded);
		});

		
		for (int i = 0; i < playerUnlockables.Count; i++)
		{
			int x = i;		// Local scope for Lambda
			InstanceIDtoAction.Add(playerUnlockables[i].selectableImage.transform.GetInstanceID(),() => isTouchingUIElement = HighlightOnUnlockableSelected(x));
		}
	}

	public void SwitchView(ViewStates newState)
	{
		previousViewState = viewState;
		viewState = newState;
		StartCoroutine(IChangeView());
	}

	/// <summary>
	/// Triggers the Mask and Fading functionality for respective ViewStates
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	private IEnumerator IChangeView()
	{
		switch (viewState)
		{
			case ViewStates.LevelLoaded:

				// Stop the Timer value updating (until we move)
				if (GameSave.LevelTimerEnabled)
					UIManager.Instance.EnableLevelTimer(false);
				
				//Debug.Log($"Timer Enabled: {GameSave.LevelTimerEnabled}. Level {GameSave.CurrentLevel} Time: {GameSave.GetLevelTimeMS(GameSave.CurrentLevel)}");

				if (previousViewState == ViewStates.LevelLoaded || previousViewState == ViewStates.LevelComplete)
				{
					ScaleMask(Scale.Up, Color.black);
					yield return new WaitForSeconds(0.5F);
					spriteRenderer.color = Color.black;
					FadeCanvasGroup(Fade.ToOpaque, gameplayCanvasGroup);
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					playerSelectionHitBox.gameObject.SetActive(true);
				}
				// If we are switching to Level Loaded from Player Selection state
				if (previousViewState == ViewStates.PlayerSelection)
				{
					LevelManager.Instance.ToggleLevel(true);
					Color temp = playerSelectHighlightMat.color;
					FadeSelectionCubes(Fade.ToTransparent, 5F);
					FadeOverlays(Fade.ToTransparent, 5F);
					yield return new WaitForSeconds(.05F);
					yield return FadeCanvasGroup(Fade.ToTransparent, playerSelectCanvasGroup, 2F);
					yield return new WaitForSeconds(.05F);
					yield return ScaleMask(Scale.Up, Color.black, 3.5F);
					playerSelectHighlightMat.color = temp;
					StopCoroutine(playerSelectScrollCoroutine);
					SetPlayerSelectionObjectVisibility(false);
					//GameManager.Instance.playerPathFollower.enabled = true;
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					tapToRestart.gameObject.SetActive(true);
					tapToRestartHitBox.gameObject.SetActive(true);
					settings.Button.transform.parent.gameObject.SetActive(true);
					playerSelectionHitBox.gameObject.SetActive(true);
				}
				else if (previousViewState == ViewStates.LevelRestart)
				{
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					playerSelectionHitBox.gameObject.SetActive(true);
					ScaleMask(Scale.Up, Color.black);
					yield return new WaitForSeconds(0.5F);
					spriteRenderer.color = Color.black;
					FadeCanvasGroup(Fade.ToOpaque, gameplayCanvasGroup);
				}
				previewCubeRotator.enabled = false;

				if (GameSave.LevelTimerEnabled)
				{
					UIManager.Instance.SetLevelTimerValue(GameSave.GetLevelTimeS(GameSave.CurrentLevel), GameSave.GetLevelTimeMS(GameSave.CurrentLevel));
					UIManager.Instance.UpdateLevelTimerUI();
				}

				if (settings.open)
				{
					settings.isAnimating = true;
					yield return StartCoroutine(AnimateSettings(false));
				}

				yield return FadeCanvasGroup(Fade.ToOpaque, settings.canvasGroup, 2.9F);
				settings.Toggle(true);
				break;

			case ViewStates.LevelComplete:
				GameManager.Instance.playerPathFollower.SetPlayerControl(false);
				ScaleMask(Scale.Down, Color.black);
				yield return new WaitForSeconds(0.5F);
				spriteRenderer.color = Color.black;
				yield return FadeCanvasGroup(Fade.ToTransparent, gameplayCanvasGroup);
				levelProgressImage.rectTransform.position = progressImageOffScreenPos;
				break;

			case ViewStates.LevelFailed:
				GameManager.Instance.playerPathFollower.SetPlayerControl(false);
				tapToRestartEnabled = false;
				tapToRestart.gameObject.SetActive(true);
				tapToRestartHitBox.gameObject.SetActive(true);
				levelPercentText = $"{(int)(GameManager.Instance.playerPathFollower.timeOnPath * 100F)}% COMPLETED";
				spriteRenderer.color = Color.black;
				FadeCanvasGroup(Fade.ToTransparent, gameplayCanvasGroup);
				ScaleMask(Scale.Down, levelFailedColour);
				yield return new WaitForSeconds(0.5F);
				spriteRenderer.color = levelFailedColour;
				yield return FadeCanvasGroup(Fade.ToOpaque, levelFailedCanvasGroup, 2F);
				tapToRestartEnabled = true;
				break;

			case ViewStates.LevelRestart:
				GameManager.Instance.playerPathFollower.SetPlayerControl(false);
				//GameManager.Instance.playerPathFollower.enabled = false;
				tapToRestart.gameObject.SetActive(false);
				tapToRestartHitBox.gameObject.SetActive(false);
				spriteRenderer.color = levelFailedColour;
				FadeCanvasGroup(Fade.ToTransparent, levelFailedCanvasGroup);
				yield return new WaitForSeconds(0.25F);
				yield return StartCoroutine(IFadeBackgroundColour(Color.black));
				yield return new WaitForSeconds(0.25F);
				GameManager.Instance.playerPathFollower.OnLevelReset();
				levelProgressImage.rectTransform.position = progressImageOffScreenPos;
				SwitchView(ViewStates.LevelLoaded);
				break;

			case ViewStates.PlayerSelection:
				playerSelectScrollCoroutine = StartCoroutine(ScrollUIImage());
				GameManager.Instance.playerPathFollower.SetPlayerControl(false);
				tapToPlay.gameObject.SetActive(false);
				tapToPlayHitBox.gameObject.SetActive(false);
				tapToRestart.gameObject.SetActive(false);
				tapToRestartHitBox.gameObject.SetActive(false);
				settings.Button.transform.parent.gameObject.SetActive(false);
				playerSelectionHitBox.gameObject.SetActive(false);
				skinIndex = 0;
				ApplyPlayerSelectionUnlockableStates();
				SetPlayerSelectionObjectVisibility(true);
				previewCubeRotator.enabled = true;
				//spriteRenderer.color = PlayerSelectColour;
				yield return ScaleMask(Scale.Down, Color.black, 3.5F);
				LevelManager.Instance.ToggleLevel(false);
				yield return new WaitForSeconds(.05F);
				FadeCanvasGroup(Fade.ToOpaque, playerSelectCanvasGroup, 3.5F);
				yield return new WaitForSeconds(.05F);
				FadeSelectionCubes(Fade.ToOpaque, 3.5F);
				FadeSlots(Fade.ToOpaque, 3.5F);
				yield return FadeOverlays(Fade.ToOpaque, 3.5F);
				break;
		}
	}

	public void UpdateLevelTimerUI()
	{
		timerSecondsText.text = timerSecondsValue.ToString();
		timerMilisecondsText.text = timerMilisecondsValue.ToString(".##");
	}

	public void SetLevelTimerUIVisibility(bool visible)
	{
		timerSecondsText.gameObject.SetActive(visible);
		timerMilisecondsText.gameObject.SetActive(visible);
	}

	public void ResetLevelTimer()
	{
		timerSecondsValue = 0;
		timerMilisecondsValue = 0;
		UpdateLevelTimerUI();
	}

	public void SetLevelTimerValue(int seconds, float miliseconds)
	{
		timerSecondsValue = Mathf.Clamp(seconds, 0, int.MaxValue);
		timerMilisecondsValue = Mathf.Clamp(miliseconds, 0, float.MaxValue);
	}

	/// <summary>
	/// Animates the Settings Drop-down UI
	/// </summary>
	private IEnumerator AnimateSettings(bool toOpen)
	{
		// FINISH FOR CLOSING
		if (toOpen) // Open
		{
			settings.slidingObject.SetActive(true);
			settings.Button.transform.RotateOverTime(this, Vector3.forward * (settings.open ? 1F : -1F) * 60F, 0.3F);
			settings.backgroundImage.rectTransform.ResizeOverTime(this, settings.backExtSize * Vector2.up, 0.3F);
			yield return new WaitForSeconds(0.15F);
			settings.vibrateImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * 40F, .1F, null);
			settings.vibrateBackgroundImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * 50F, .1F, null);
			yield return new WaitForSeconds(0.15F);
			settings.timerImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * 40F, .1F, null);
			yield return settings.timerBackgroundImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * 50F, .1F, null);

			settings.isAnimating = false;
			settings.open = true;
		}
		else // Close
		{
			settings.Button.transform.RotateOverTime(this, Vector3.forward * (settings.open ? 1F : -1F) * 60F, 0.3F);
			settings.timerImage.rectTransform.ResizeOverTime(this, -CanvasUtils.Positive2D * 40F, .1F, null);
			settings.timerBackgroundImage.rectTransform.ResizeOverTime(this, -CanvasUtils.Positive2D * 50F, .1F, null);
			yield return new WaitForSeconds(0.15F);
			settings.vibrateImage.rectTransform.ResizeOverTime(this, -CanvasUtils.Positive2D * 40F, .1F, null);
			settings.vibrateBackgroundImage.rectTransform.ResizeOverTime(this, -CanvasUtils.Positive2D * 50F, .1F, null);
			yield return settings.backgroundImage.rectTransform.ResizeOverTime(this, settings.backExtSize * Vector2.down, 0.3F);

			settings.isAnimating = false;
			settings.open = false;
		}
	}

	/// <summary>
	/// Wrapper function for IScaleMask
	/// </summary>
	private Coroutine ScaleMask(Scale type, Color newColour, float customSpeed = 0)
	{
		return StartCoroutine(IScaleMask(type, newColour, customSpeed));
	}

	/// <summary>
	/// Scales In or Out the UI Mask with Color and Speed. Size is fixed
	/// Optionally, start the scaling with a new Colour
	/// If we are fading for a level reset, use a lighter background colour
	/// </summary>
	private IEnumerator IScaleMask(Scale type, Color newColour, float customSpeed = 0)
	{
		maskScaleState = ScaleStates.InProgress;
		spriteRenderer.color = newColour;

		float t = 0;
		float speed = customSpeed > 0 ? customSpeed : GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		// the Sprite size from start to end
		Vector3 startSpriteScale = type == Scale.Down ? maskFadeSpriteMaxSize : Vector3.zero;
		Vector3 endSpriteScale = type == Scale.Down ? Vector3.zero : maskFadeSpriteMaxSize;

		while (t < 1)
		{
			t += Time.deltaTime * speed;
			maskFadeSprite.transform.localScale = Vector3.Lerp(startSpriteScale, endSpriteScale, t);
			yield return null;
		}

		maskScaleState = ScaleStates.Inactive;
	}

	/// <summary>
	/// Wrapper function for IFadeBackgroundColour
	/// </summary>
	private Coroutine FadeBackgroundColour(Color newColour, float customSpeed = 0)
	{
		return StartCoroutine(IFadeBackgroundColour(newColour, customSpeed));
	}

	/// <summary>
	/// Fade the Background's colour 
	/// </summary>
	private IEnumerator IFadeBackgroundColour(Color newColour, float customSpeed = 0)
	{
		float t = 0;
		float speed = customSpeed > 0 ? customSpeed : GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		Color originalCol = spriteRenderer.color;

		while(t < 1)
		{
			t += Time.deltaTime * speed;
			spriteRenderer.color = Color.Lerp(originalCol, newColour, t);
			yield return null;
		}

	}
	/// <summary>
	/// Wrapper function for IFadePlayerSelectionItems
	/// </summary>
	private Coroutine FadeCanvasGroup(Fade type, CanvasGroup canvasGroup, float fadeSpeed = 0)
	{
		return StartCoroutine(IFadeCanvasGroup(type, canvasGroup, fadeSpeed));
	}

	/// <summary>
	/// Fades a canvas group In or Out with a speed
	/// </summary>
	private IEnumerator IFadeCanvasGroup(Fade type, CanvasGroup canvasGroup, float speed)
	{
		float t = 0;
		float x = speed > 0 ? speed : GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		while (t < 1)
		{
			t += Time.deltaTime * x;
			canvasGroup.alpha = type == Fade.ToOpaque ? t : 1F - t;
			yield return null;
		}
	}

	/// <summary>
	/// Reset the Player Selection Cubes to be transparent
	/// </summary>
	private void ResetPlayerSelectionCubes()
	{
		for (int i = 0; i < playerUnlockables.Count; i++)
		{
			playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color = new Color(
				playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.r,
				playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.g,
				playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.b,
				0);

			previewCubeMeshRenderer.sharedMaterial.color = new Color(
				previewCubeMeshRenderer.sharedMaterial.color.r,
				previewCubeMeshRenderer.sharedMaterial.color.g,
				previewCubeMeshRenderer.sharedMaterial.color.b,
				0);
		}
	}

	/// <summary>
	/// Enables/Disables Gameobjects in the Player Selection UI which use transparency
	/// </summary>
	private void SetPlayerSelectionObjectVisibility(bool visible)
	{
		playerSelectCanvasGroup.gameObject.SetActive(visible);

		foreach (PlayerUnlockable unlockable in playerUnlockables)
		{
			unlockable.cubeMeshRenderer.gameObject.SetActive(visible);
		}
	}

	/// <summary>
	/// Adjust the UI for the Player Skin unlockables currently unlocked by the player
	/// </summary>
	public void ApplyPlayerSelectionUnlockableStates()
	{
        for (int i = 0; i < playerUnlockables.Count; i++)
        {
			if (GameSave.IsPlayerSkinUnlocked(i))
				playerUnlockables[i].overlay.gameObject.SetActive(false);
			else
				playerUnlockables[i].overlay.material = playerSelectLockedMat;
		}
	}

	/// <summary>
	/// Processes the Highlighting for the last and current touched UI Unlockable
	/// </summary>
	private bool HighlightOnUnlockableSelected(int index)
	{
		// Animate Item Box when touched - Scale down then back up
		if (playerUnlockables[index].isAnimating == false)
		{
			playerUnlockables[index].isAnimating = true;		// We are animating
			Vector2 sizeDelta = playerUnlockables[index].selectableImage.rectTransform.sizeDelta;	// Cache the og size

			// Scale down, and scale up once complete
			playerUnlockables[index].selectableImage.rectTransform.ResizeOverTime(this, sizeDelta * CanvasUtils.Negative2D * 0.20F, .075F, null, () =>
			{
				playerUnlockables[index].selectableImage.rectTransform.ResizeOverTime(this, sizeDelta * CanvasUtils.Positive2D * 0.20F, .075F, null, () =>
				{
					playerUnlockables[index].isAnimating = false;
				});
			});
		}

		// If we pick the same Item, return
		if (index == skinIndex) return true;

		// If we have previous selected an unlockable and its not unlocked, set it to the locked colour
		if (skinIndex != -1 && !GameSave.IsPlayerSkinUnlocked(skinIndex))
			playerUnlockables[skinIndex].overlay.material = playerSelectLockedMat;

		skinIndex = index;

		// If we have already unlocked this skin, hide the purchase button, apply the skin and return
		if (GameSave.IsPlayerSkinUnlocked(skinIndex))
		{
			if (playerSelectionPurchaseButton.activeInHierarchy)
				playerSelectionPurchaseButton.SetActive(false);

			// Swap the current Materials
			SwapPlayerColoursOnSelection();

			Debug.Log("Skin unlocked and Applied");

			return true;
		}
		else
		{
			// If we have not unlocked the skin, highlight the unlockable
			playerUnlockables[skinIndex].overlay.material = playerSelectHighlightMat;
			Debug.Log("Skin locked. Using player select highlight mat");

			// If the purchase button is not visible, make it visible
			if (!playerSelectionPurchaseButton.activeInHierarchy)
				playerSelectionPurchaseButton.SetActive(true);

			// Set the cost of the unlockable and activate the dimmed overlay if we can't afford it
			playerSelectionPurchaseText.text = playerUnlockables[skinIndex].cost.ToString();
			playerSelectionPurchaseButtonOverlay.SetActive(GameSave.CoinCount < playerUnlockables[skinIndex].cost);
		}

		return true;
	}

	/// <summary>
	/// Swaps the changeable colours when the Player selects a unlocked material
	/// </summary>
	private void SwapPlayerColoursOnSelection()
	{
		previewCubeMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.playerMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.PlayerTrailColour = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerParticleMaterial.color = LevelManager.Instance.GetOffsetColour(playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color, -100F);
		GameSave.SetLastSkinIndex(skinIndex);
	}

	/// <summary>
	/// Defaults the Player Colours
	/// </summary>
	public void SwapPlayerColoursToDefault()
	{
		previewCubeMeshRenderer.sharedMaterial.color = playerUnlockables[0].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.playerMeshRenderer.sharedMaterial.color = playerUnlockables[0].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.PlayerTrailColour = playerUnlockables[0].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerParticleMaterial.color = LevelManager.Instance.GetOffsetColour(playerUnlockables[0].cubeMeshRenderer.sharedMaterial.color, -100F);
		GameSave.SetLastSkinIndex(0);
	}

	/// <summary>
	/// Applies the last unlocked skin on Game Startup
	/// </summary>
	/// <param name="skinIndex"></param>
	public void ApplyLastUnlockedSkin(int skinIndex)
	{
		previewCubeMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.playerMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.PlayerTrailColour = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerParticleMaterial.color = LevelManager.Instance.GetOffsetColour(playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color, -100F);
		Debug.Log($"LAI Apply: {skinIndex}");
	}

	/// <summary>
	/// Wrapper function for IFadePlayerSelectionItems
	/// </summary>
	private Coroutine FadeSelectionCubes(Fade type, float fadeSpeed)
	{
		return StartCoroutine(IFadeSelectionCubes(type, fadeSpeed));
	}

	private Coroutine FadeOverlays(Fade type, float fadeSpeed)
	{
		return StartCoroutine(IFadeOverlays(type, fadeSpeed));
	}

	private Coroutine FadeSlots(Fade fade, float fadeSpeed)
	{
		return StartCoroutine(IFadeSlots(fade, fadeSpeed));
	}

	/// <summary>
	/// Fade the Selectable Player Cubes and Preview Cube In or Out with speed
	/// </summary>
	private IEnumerator IFadeSelectionCubes(Fade type, float fadeSpeed)
	{
		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime * fadeSpeed;
			for (int i = 0; i < playerUnlockables.Count; i++)
			{
				playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color = new Color(playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.r,
																					   playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.g,
																					   playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.b, 
																					   type == Fade.ToOpaque ? t : 1F - t);

				
			}

			previewCubeMeshRenderer.sharedMaterial.color = new Color(previewCubeMeshRenderer.sharedMaterial.color.r,
																	 previewCubeMeshRenderer.sharedMaterial.color.g,
																	 previewCubeMeshRenderer.sharedMaterial.color.b,
																	 type == Fade.ToOpaque ? t : 1F - t);

			yield return null;
		}
	}


	/// <summary>
	/// Fade transparency between two specific alpha values for Unlockable overlays
	/// </summary>
	private IEnumerator IFadeOverlays(Fade type, float fadeSpeed)
	{
		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime * fadeSpeed;
			for (int i = 0; i < playerUnlockables.Count; i++)
			{
				playerUnlockables[i].overlay.material.color = new Color(playerUnlockables[i].overlay.material.color.r,
																		playerUnlockables[i].overlay.material.color.g,
																		playerUnlockables[i].overlay.material.color.b,
																		type == Fade.ToOpaque ? Mathf.Lerp(0, playerSelectLockedColourCached.a, t) : Mathf.Lerp(playerSelectLockedColourCached.a, 0, t));
				
			}
			yield return null;
		}
	}

	private IEnumerator IFadeSlots (Fade type, float fadeSpeed)
	{
		float t = 0;
		while (t < 1)
		{
			t += Time.deltaTime * fadeSpeed;
			for (int i = 0; i < playerUnlockables.Count; i++)
			{
				playerUnlockables[i].selectableImage.material.color = new Color(playerUnlockables[i].selectableImage.material.color.r,
																		playerUnlockables[i].selectableImage.material.color.g,
																		playerUnlockables[i].selectableImage.material.color.b,
																		type == Fade.ToOpaque ? 1 : 0);
			}
			yield return null;
		}
	}

	/// <summary>
	/// Get the Fade Speed for the Level Difficulty. Harder levels have a faster speed.
	/// </summary>
	private static float GetFadeSpeed(Level.LevelDifficulty levelDifficulty)
	{
		switch (levelDifficulty)
		{
			case Level.LevelDifficulty.Beginner: return 1.45F;
			case Level.LevelDifficulty.Intermediate: return 1.60F;
			case Level.LevelDifficulty.Hard: return 1.75F;
			case Level.LevelDifficulty.Impossible: return 2F;
		}

		return 1F;
	}

	/// <summary>
	/// Update our level progress image to match our travelled distance in the level
	/// </summary>
	public void UpdateLevelProgressUI(float distance, float vertCount)
	{
		levelProgressImage.fillAmount = GameManager.Instance.playerPathFollower.timeOnPath;
	}

	/// <summary>
	/// Update the UI Coins over time using the IUpdateUICoins Method
	/// </summary>
	/// <param name="oldCoinCount"></param>
	public void UpdateUICoins(float oldCoinCount, float speed = 0)
	{
		StartCoroutine(IUpdateUICoins(oldCoinCount, speed));
	}

	/// <summary>
	/// Update UI Coins over time
	/// </summary>
	private IEnumerator IUpdateUICoins(float oldCoinCount, float speed = 0F)
	{
		float sign = oldCoinCount < GameSave.CoinCount ? 1 : -1;

		// While the old coin coint is not equal to the new one, update our display value
		while ((int)oldCoinCount != GameSave.CoinCount)
		{
			oldCoinCount += Time.deltaTime * (speed == 0F ? Instance.coinCounterUpdateSpeed : speed) * sign;
			Instance.coinCounterText.text = ((int)oldCoinCount).ToString();
			yield return null;
		}
	}

	private IEnumerator ScrollUIImage()
	{
		switch (LevelManager.Instance.currentLevel.Difficulty)
		{
			case Level.LevelDifficulty.Beginner:
				playerSelectbackgroundImage.color = LevelManager.Instance.beginnerWaterColour;
				break;
			case Level.LevelDifficulty.Intermediate:
				playerSelectbackgroundImage.color = LevelManager.Instance.IntermediateWaterColour;
				break;
			case Level.LevelDifficulty.Hard:
				playerSelectbackgroundImage.color = LevelManager.Instance.hardWaterColour;
				break;
			case Level.LevelDifficulty.Impossible:
				playerSelectbackgroundImage.color = LevelManager.Instance.impossibleWaterColour;
				break;
		}

		while (true)
		{
			playerSelectScrollingBackgroundImage.mainTextureOffset += Vector2.left * backgroundImageScrollSpeed * Time.deltaTime;
			yield return null;
		}
	}
}