// Author: Lewis Dawson
// Title: UITouch Graphics Raycast Manager
// Description: Processes UI touch functionality using graphics raycasting for specified objects using their InstanceID()
// Object IDs are linked in Awake() to a Touch Filter. Filters functionality is then processed on object touch
// Object can choose to report as a touch or non-touch. For example, the "Tap-to-play" object should report as non-touch, so
// we can continue to touch the screen for movement
// Limitations: Currently only work with 1 Canvas. Filters are fixed - can't be added at runtime

// RENAME TO UI MANAGER

using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CandyCoded.HapticFeedback.Android;
using System;

public enum FadeTypes
{
    ToTransparent,
	ToOpaque,
}

public enum ScaleTypes
{
	Up,
	Down,
}

public enum ScaleStates
{
    Inactive,
    InProgress,
}



public class UITouch : Singleton<UITouch>
{
	public bool touchingOverFrames = false;		// Used to filter out touch spamming by holding down a touch
	public bool isTouchingUIElement = false;	// Determines if we are touching a UI item

	public enum TouchFilters
    {
        Settings,
        SettingsWindow,
        PathFollowerToggle,
        DebugToggle,
        TapToPlay,
		TapToRestart,
    }

    private Dictionary<int, TouchFilters> InstanceIDtoFilter = new Dictionary<int, TouchFilters>();

    // Graphics raycasting
    [Header("Raycasting Systems")]
    [SerializeField] private GraphicRaycaster gameplayUIRaycaster = null;
	[SerializeField] private GraphicRaycaster levelFailedUIRaycaster = null;
	[SerializeField] private PointerEventData uiPointerEventData = null;
    [SerializeField] private EventSystem uiEventSystem = null;

	[Header("UI Elements")]
	public Canvas gameplayCanvas;
	public RectTransform gameplayCanvasTransform;
    public CanvasGroup gameplayCanvasGroup;
	public CanvasGroup levelFailedCanvasGroup;
	[SerializeField] TMP_Text levelPercentTMPText = null;
	public string levelPercentText
	{
		set { levelPercentTMPText.text = value; }
	}
	public TextMeshProUGUI progressText; // MOVE TO UI SCRIPT


	public enum ViewStates
	{
		LoadLevel,
		LevelComplete,
		LevelFailed,
		LevelRestart
	}
	private static ViewStates viewState;
	public static ViewStates GetViewState => viewState;

	[Header("Gameplay View Interactables")]
	public Settings settings;
	[System.Serializable]
	public struct Settings
	{
		public Transform Button;
		public float backgroundHeight;
		public Image backgroundImage;
		public Image testImage;

		public bool open;
		public bool isAnimating;
	}

	[SerializeField] TextMeshProUGUI vSyncText = null;
	public TextBounce tapToPlay;
    public RectTransform tapToPlayHitBox;


	[Header("Restart View Interactables")]
	public TextBounce tapToRestart;
	public RectTransform tapToRestartHitBox;

    [SerializeField] private Timer debugTimer = null;

    [SerializeField] private SpriteRenderer spriteRenderer = null;
	[SerializeField] private Transform maskFadeSprite;
	[SerializeField] private Color levelFailedColour = Color.clear;
    private Vector3 maskFadeSpriteMaxSize = Vector3.zero;
    public ScaleStates maskScaleState;
	public RectPositioner imagePositioner;


	new private void Awake()
	{
		base.Awake();

		FPSDispay.Instance.enabled = false;
		vSyncText.gameObject.SetActive(false);

		InstanceIDtoFilter.Add(settings.Button.GetInstanceID(), TouchFilters.Settings);
		InstanceIDtoFilter.Add(tapToPlayHitBox.GetInstanceID(), TouchFilters.TapToPlay);
		InstanceIDtoFilter.Add(tapToRestartHitBox.GetInstanceID(), TouchFilters.TapToRestart);

		maskFadeSpriteMaxSize = maskFadeSprite.transform.localScale;
		gameplayCanvasGroup.alpha = 0;
	}

	// Each frame, if we have provided Mouse/Touch input, and aren't touching over multiple frames,
	// Graphics Raycast our screen with the touch position. If we touch an object with a mathcing InstanceID, process its filter
	// Store all hit objects hit, in our raycast list
	private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) || Input.touchCount > 0)
        {
            // If we touched the screen over multiple frames, don't accept input
            if (touchingOverFrames)
                return;

            // If we didn't touch screen in last update, We are now touching over previous frames with this frame
            touchingOverFrames = true;

            // Set up the new Pointer Event and assign the mouse position
            uiPointerEventData = new PointerEventData(uiEventSystem);
            uiPointerEventData.position = Input.mousePosition;

            // Create a new list of results for our raycast
            List<RaycastResult> hitResults = new List<RaycastResult>();

			// Raycast using the Graphics Raycaster and mouse click position dependant on view state
			if (viewState == ViewStates.LevelFailed)
				levelFailedUIRaycaster.Raycast(uiPointerEventData, hitResults);
			else
				gameplayUIRaycaster.Raycast(uiPointerEventData, hitResults);

            // Do the appropriate action for the hit object. We only want to hit the top-most item. We only break if we find a dictionary match.
            // By default, the list is sorted from first to last from index 0. E.g, Items layered beneath are at the end of the array.
            for (int i = 0; i < hitResults.Count; i++)
            {
                if (InstanceIDtoFilter.TryGetValue(hitResults[i].gameObject.transform.GetInstanceID(), out TouchFilters foundFilter))
                {
                    isTouchingUIElement = ProcessFilter(foundFilter);

                    if (isTouchingUIElement)
                        break;
                }
                else
                {
					//Debug.Log(Utils.ColourText($"Raycast hit {i}: {hitResults[i].gameObject.name} is not registered as clickable", Color.green));
                }
            }
        }
        else
        {
            touchingOverFrames = false;
            isTouchingUIElement = false;
        }
    }

    // Filter the appropriate action for the UI items we touched
    private bool ProcessFilter(TouchFilters filter)
    {
		switch (filter)
		{
			case TouchFilters.Settings:

				if (!settings.isAnimating)
				{
					settings.isAnimating = true;
					settings.Button.RotateOverTime(this, Vector3.forward * (settings.open ? 1F : -1F) * 60F, .3F);
					
					List<CanvasUtils.TimedAction> actions = new List<CanvasUtils.TimedAction>(){
						new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Single, settings.open ? 0.2F: 0.7F, () => settings.testImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * (settings.open ? 1F : -1F) * 40F, .1F, null)),
						new CanvasUtils.TimedAction( CanvasUtils.TimedAction.Modes.Single, 1F, () => settings.isAnimating = false)
					};

					settings.backgroundImage.rectTransform.ResizeOverTimeWithActions(this, (settings.open ? -1F : 1F) * settings.backgroundHeight * Vector3.up, .3F, actions);
					settings.open = !settings.open;
				}
				return true;

			case TouchFilters.SettingsWindow:
				Utils.Log("Touched Settings Window");
				return true;

			// When we touch the tap to play Hitbox, hide it and start movement
			// While we touch Tap-To-Play, we don't want to block background touch for movement, so report back as false
			case TouchFilters.TapToPlay:
				tapToPlay.gameObject.SetActive(false);
				tapToPlayHitBox.gameObject.SetActive(false);
				LevelManager.Instance.progressImage.rectTransform.Move(this, LevelManager.Instance.progressImageOffscreenPos, LevelManager.Instance.progressImageonscreenPos, 1.2F, CurveType.Exponential);
				return false;

			case TouchFilters.TapToRestart:
				tapToRestart.gameObject.SetActive(false);
				tapToRestartHitBox.gameObject.SetActive(false);
				SwitchView(ViewStates.LevelRestart);
				return false;
		}
        return false;
    }

	public void SwitchView(ViewStates state)
	{
		viewState = state;
		PreViewUpdate();
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
			case ViewStates.LoadLevel:
				StartCoroutine(IScaleMask(ScaleTypes.Up));
				yield return new WaitForSeconds(0.5F);
				StartCoroutine(IFadeCanvasGroup(FadeTypes.ToOpaque, gameplayCanvasGroup, Color.black));
				break;

			case ViewStates.LevelComplete:
				StartCoroutine(IScaleMask(ScaleTypes.Down));
				yield return new WaitForSeconds(0.5F);
				StartCoroutine(IFadeCanvasGroup(FadeTypes.ToTransparent, gameplayCanvasGroup, Color.black));
				break;

			case ViewStates.LevelFailed:
				StartCoroutine(IFadeCanvasGroup(FadeTypes.ToTransparent, gameplayCanvasGroup, Color.black));
				StartCoroutine(IScaleMask(ScaleTypes.Down, true));
				yield return new WaitForSeconds(0.5F);
				StartCoroutine(IFadeCanvasGroup(FadeTypes.ToOpaque, levelFailedCanvasGroup, levelFailedColour));
				break;

			case ViewStates.LevelRestart:
				StartCoroutine(IFadeCanvasGroup(FadeTypes.ToTransparent, levelFailedCanvasGroup, levelFailedColour));
				yield return new WaitForSeconds(0.25F);
				yield return StartCoroutine(IFadeMaskColour(Color.black));
				GameManager.Instance.playerPathFollower.OnLevelReset();
				SwitchView(ViewStates.LoadLevel);
				break;
		}
	}

	/// <summary>
	/// Sets data for a specific view before it is presented
	/// </summary>
	/// <param name="state"></param>
	private void PreViewUpdate()
	{
		switch (viewState)
		{
			case ViewStates.LoadLevel:
				break;

			case ViewStates.LevelComplete:
				// Open Chest, Fire out coins, show level complete screen
				break;

			case ViewStates.LevelFailed:
				levelPercentText = $"{GameManager.Instance.playerPathFollower.pathPercentComplete}% COMPLETED";
				tapToRestart.gameObject.SetActive(true);
				tapToRestartHitBox.gameObject.SetActive(true);
				break;
		}
	}

	/// <summary>
	/// Fade in or out from start to end scale using Vector3.Lerp and linear time
	/// Uses an offset (delay) for the Canvas Alpha lerp.
	/// If we are fading for a level reset, use a lighter background colour
	/// </summary>
	/// <param name="type"></param>
	/// <param name="speed"></param>
	/// <returns></returns>
	private IEnumerator IScaleMask(ScaleTypes type, bool levelFailed = false)
	{
		maskScaleState = ScaleStates.InProgress;
		spriteRenderer.color = levelFailed ? levelFailedColour : Color.black;

		float t = 0;
		float speed = GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		// the Sprite size from start to end
		Vector3 startSpriteScale = type == ScaleTypes.Down ? maskFadeSpriteMaxSize : Vector3.zero;
        Vector3 endSpriteScale = type == ScaleTypes.Down ? Vector3.zero : maskFadeSpriteMaxSize;

		while ((t += Time.deltaTime) < 1)
		{
			maskFadeSprite.transform.localScale = Vector3.Lerp(startSpriteScale, endSpriteScale, t * speed);
			yield return null;
		}

		maskScaleState = ScaleStates.Inactive;
	}

	private IEnumerator IFadeMaskColour(Color newColour)
	{
		float t = 0;
		float speed = GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		Color originalCol = spriteRenderer.color;

		while((t += Time.deltaTime) < 1)
		{
			spriteRenderer.color = Color.Lerp(originalCol, newColour, t * speed);
			yield return null;
		}

	}

	private IEnumerator IFadeCanvasGroup(FadeTypes type, CanvasGroup canvasGroup, Color newColour)
	{
		spriteRenderer.color = newColour;

		float t = 0;
		float speed = GetFadeSpeed(LevelManager.Instance.currentLevel.Difficulty);

		float startAlpha = type == FadeTypes.ToOpaque ? 0 : 1;
		float endAlpha	 = type == FadeTypes.ToOpaque ? 1 : 0;

		while ((t += Time.deltaTime) < 1)
		{
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t * speed);
			yield return null;
		}
	}


	/// <summary>
	/// Get the Fade Speed for the Level Difficulty. Harder levels have a faster speed.
	/// </summary>
	/// <param name="levelDifficulty"></param>
	/// <returns></returns>
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

	// The method used to Open the Android Keyboard to edit settings
	//public IEnumerator WaitForKeyboard(TouchFilters filter)
	//{
	//        // Stops other requests to open keyboard until we are done with this one
	//        keyboardOpenRequested = true;

	//        // If not mobile, return
	//#if UNITY_EDITOR
	//        keyboardOpenRequested = false;
	//       yield return null;
	//#endif

	//switch (filter)
	//{
	//case TouchFilters.VSync:
	//    keyboard = TouchScreenKeyboard.Open(QualitySettings.vSyncCount.ToString(), TouchScreenKeyboardType.NumberPad);
	//    break;

	//case TouchFilters.PathFollowerMaxSpeed:
	//    keyboard = TouchScreenKeyboard.Open(GameManager.instance.pathfollower.maxSpeed.ToString(), TouchScreenKeyboardType.NumberPad);
	//    break;

	//case TouchFilters.PathFollowerAccel:
	//    keyboard = TouchScreenKeyboard.Open(GameManager.instance.pathfollower.accelerationMultiplier.ToString(), TouchScreenKeyboardType.NumberPad);
	//    break;
	//}

	// While the keyboard is visible, stop gameplay and wait
	//while (keyboard.status == TouchScreenKeyboard.Status.Visible)
	//{
	//    GameManager.instance.gameplayEnabled = false;
	//    yield return new WaitForEndOfFrame();
	//}

	//if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
	//{
	//    keyboardOpenRequested = false;
	//    yield return null;
	//}

	//int parsedNum = 0;

	//switch (filter)
	//{
	//case TouchFilters.VSync:
	//    if (int.TryParse(keyboard.text, out parsedNum))
	//    {
	//        if (keyboard.text == "0")
	//        {
	//            Application.targetFrameRate = int.MaxValue;
	//        }

	//        vSyncText.text = $"vSync: {keyboard.text}";

	//        QualitySettings.vSyncCount = (parsedNum);
	//    }
	//    break;

	//case TouchFilters.PathFollowerMaxSpeed:

	//    if (int.TryParse(keyboard.text, out parsedNum))
	//    {
	//        GameManager.instance.pathfollower.maxSpeed = parsedNum;
	//        maxMoveSpeed.text = $"Max Speed: {GameManager.instance.pathfollower.maxSpeed}";
	//    }
	//    break;

	//case TouchFilters.PathFollowerAccel:
	//    if (int.TryParse(keyboard.text, out parsedNum))
	//    {
	//        GameManager.instance.pathfollower.accelerationMultiplier = parsedNum;
	//        moveAcceleration.text = $"Acceleration: {GameManager.instance.pathfollower.accelerationMultiplier}";
	//    }
	//    break;
	//}

	//    GameManager.instance.gameplayEnabled = true;
	//}
}