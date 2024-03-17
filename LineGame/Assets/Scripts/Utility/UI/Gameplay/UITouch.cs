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
using System;

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
		ToPlayerSelectionWindow,
		FromPlayerSelectionWindow,
		PlayerSelectionPurchaseButton,
		PlayerSelectable1,
		PlayerSelectable2,
		PlayerSelectable3,
		PlayerSelectable4,
		PlayerSelectable5,
		PlayerSelectable6,
		PlayerSelectable7,
		PlayerSelectable8,
		PlayerSelectable9,
	}

    private Dictionary<int, TouchFilters> InstanceIDtoFilter = new Dictionary<int, TouchFilters>(); 
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
	[SerializeField] TMP_Text levelPercentTMPText = null;
	public string levelPercentText
	{
		set { levelPercentTMPText.text = value; }
	}
	public TextMeshProUGUI progressText;
	public Vector2 progressImageOffScreenPos = Vector2.zero;
	public Vector2 progressImageOnScreenPos = Vector2.zero;
	public SlicedFilledImage levelProgressImage = null;

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
	public RectTransform playerSelectionHitBox;
	public RectTransform PlayerSelectionReturnHitBox;


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

	[Header("Player Selection Variables")]
	public CanvasGroup playerSelectCanvasGroup = null;
	public Color PlayerSelectColour;
	[SerializeField] private Material playerSelectHighlightMat;
	[SerializeField] private Material playerSelectLockedMat;
	[SerializeField] private MeshRenderer previewCubeMeshRenderer;
	public Rotate previewCubeRotator;
	[SerializeField] private GameObject playerSelectionPurchaseButton;
	[SerializeField] private TextMeshProUGUI playerSelectionPurchaseText;
	[SerializeField] private GameObject playerSelectionPurchaseButtonOverlay;
	[SerializeField] List<PlayerUnlockable> playerUnlockables;
	public int playerUnlockCount => playerUnlockables.Count;
	private int skinIndex = -1;



	new private void Awake()
	{
		base.Awake();

		FPSDispay.Instance.enabled = false;
		vSyncText.gameObject.SetActive(false);

		AddFilterableObjects();

		maskFadeSpriteMaxSize = maskFadeSprite.transform.localScale;
		gameplayCanvasGroup.alpha = 0;

		// Cache the positions for our Progress Image

		progressImageOffScreenPos = CanvasUtils.GetPos(levelProgressImage.rectTransform, CanvasPositions.Top, Instance.gameplayCanvas.scaleFactor, Instance.gameplayCanvasTransform);
		progressImageOnScreenPos = Instance.gameplayCanvasTransform.GetPos(Instance.imagePositioner.values);
		levelProgressImage.rectTransform.position = progressImageOffScreenPos;

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
			else if (viewState == ViewStates.PlayerSelection)
				playerSelectUIRaycaster.Raycast(uiPointerEventData, hitResults);
			else
				gameplayUIRaycaster.Raycast(uiPointerEventData, hitResults);

			// TEST
			//for (int i = 0; i < hitResults.Count; i++)
			//{
			//	bool isValid = InstanceIDtoAction.ContainsKey(hitResults[i].gameObject.transform.GetInstanceID());

			//	if (isValid)
			//	{
			//		InstanceIDtoAction[hitResults[i].gameObject.transform.GetInstanceID()].Invoke();

			//		if(isTouchingUIElement)
			//			break;
			//	}
			//}
			// TEST ^^^

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

	/// <summary>
	/// Assigns the touchable/interactable Objects to their filters for custom functionality
	/// InstanceID's have to be set using the Transform Component of an Object
	/// </summary>
	private void AddFilterableObjects()
	{
		//InstanceIDtoAction.Add(settings.Button.GetInstanceID(), () => { Debug.Log("Settings Test!"); isTouchingUIElement = true; });
		InstanceIDtoFilter.Add(settings.Button.GetInstanceID(), TouchFilters.Settings);
		InstanceIDtoFilter.Add(tapToPlayHitBox.GetInstanceID(), TouchFilters.TapToPlay);
		InstanceIDtoFilter.Add(tapToRestartHitBox.GetInstanceID(), TouchFilters.TapToRestart);
		InstanceIDtoFilter.Add(playerSelectionHitBox.GetInstanceID(), TouchFilters.ToPlayerSelectionWindow);
		InstanceIDtoFilter.Add(playerSelectionPurchaseButton.transform.GetInstanceID(), TouchFilters.PlayerSelectionPurchaseButton);
		InstanceIDtoFilter.Add(PlayerSelectionReturnHitBox.GetInstanceID(), TouchFilters.FromPlayerSelectionWindow);

		InstanceIDtoFilter.Add(playerUnlockables[0].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable1);
		InstanceIDtoFilter.Add(playerUnlockables[1].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable2);
		InstanceIDtoFilter.Add(playerUnlockables[2].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable3);
		InstanceIDtoFilter.Add(playerUnlockables[3].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable4);
		InstanceIDtoFilter.Add(playerUnlockables[4].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable5);
		InstanceIDtoFilter.Add(playerUnlockables[5].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable6);
		InstanceIDtoFilter.Add(playerUnlockables[6].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable7);
		InstanceIDtoFilter.Add(playerUnlockables[7].selectableImage.transform.GetInstanceID(), TouchFilters.PlayerSelectable8);

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
						new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Passive, settings.open ? 0.2F: 0.7F, () => settings.testImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * (settings.open ? 1F : -1F) * 40F, .1F, null)),
						new CanvasUtils.TimedAction( CanvasUtils.TimedAction.Modes.Passive, 1F, () => settings.isAnimating = false)
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
				Instance.levelProgressImage.rectTransform.Move(this, Instance.progressImageOffScreenPos, Instance.progressImageOnScreenPos, 1.2F, CurveType.Exponential);
				playerSelectionHitBox.gameObject.SetActive(false);
				return false;

			case TouchFilters.TapToRestart:
				SwitchView(ViewStates.LevelRestart);
				return false;

			case TouchFilters.ToPlayerSelectionWindow:
				SwitchView(ViewStates.PlayerSelection);
				return true;

			case TouchFilters.FromPlayerSelectionWindow:
				SwitchView(ViewStates.LevelLoaded);
				break;

			case TouchFilters.PlayerSelectionPurchaseButton:
				GameSave.SetPlayerSkinUnlocked(skinIndex, true);
				playerSelectionPurchaseButton.SetActive(false);
				playerUnlockables[skinIndex].overlay.gameObject.SetActive(false);
				float oldCoinCount = GameSave.CoinCount;
				GameSave.CoinCount -= playerUnlockables[skinIndex].cost;
				GameSave.Save();
				UpdateUICoins(oldCoinCount);
				SwapColoursOnSelection();
				break;

			case TouchFilters.PlayerSelectable1:
				return HighlightOnUnlockableSelected(0);

			case TouchFilters.PlayerSelectable2:
				return HighlightOnUnlockableSelected(1);

			case TouchFilters.PlayerSelectable3:
				return HighlightOnUnlockableSelected(2);

			case TouchFilters.PlayerSelectable4:
				return HighlightOnUnlockableSelected(3);

			case TouchFilters.PlayerSelectable5:
				return HighlightOnUnlockableSelected(4);

			case TouchFilters.PlayerSelectable6:
				return HighlightOnUnlockableSelected(5);

			case TouchFilters.PlayerSelectable7:
				return HighlightOnUnlockableSelected(6);

			case TouchFilters.PlayerSelectable8:
				return HighlightOnUnlockableSelected(7);
		}
        return false;
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
				if (previousViewState == ViewStates.LevelLoaded || previousViewState == ViewStates.LevelComplete)
				{
					ScaleMask(Scale.Up, Color.black);
					yield return new WaitForSeconds(0.5F);
					spriteRenderer.color = Color.black;
					FadeCanvasGroup(Fade.ToOpaque, gameplayCanvasGroup);
				}
				// If we are switching to Level Loaded from Player Selection state
				if (previousViewState == ViewStates.PlayerSelection)
				{
					yield return FadeSelectionItems(Fade.ToTransparent, 5F);
					FadeCanvasGroup(Fade.ToTransparent, playerSelectCanvasGroup, 5F);
					yield return new WaitForSeconds(.05F);
					yield return ScaleMask(Scale.Up, PlayerSelectColour, 3.5F);

					GameManager.Instance.playerPathFollower.enabled = true;
					previewCubeRotator.enabled = false;
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					tapToRestart.gameObject.SetActive(true);
					tapToRestartHitBox.gameObject.SetActive(true);
					settings.Button.parent.gameObject.SetActive(true);
					playerSelectionHitBox.gameObject.SetActive(true);
				}
				else if (previousViewState == ViewStates.LevelRestart)
				{
					Debug.Log("Hit");
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					playerSelectionHitBox.gameObject.SetActive(true);
					ScaleMask(Scale.Up, Color.black);
					yield return new WaitForSeconds(0.5F);
					spriteRenderer.color = Color.black;
					FadeCanvasGroup(Fade.ToOpaque, gameplayCanvasGroup);
				}
				break;

			case ViewStates.LevelComplete:
				ScaleMask(Scale.Down, Color.black);
				yield return new WaitForSeconds(0.5F);
				spriteRenderer.color = Color.black;
				FadeCanvasGroup(Fade.ToTransparent, gameplayCanvasGroup);
				break;

			case ViewStates.LevelFailed:
				levelPercentText = $"{GameManager.Instance.playerPathFollower.pathPercentComplete}% COMPLETED";
				tapToRestart.gameObject.SetActive(true);
				tapToRestartHitBox.gameObject.SetActive(true);
				spriteRenderer.color = Color.black;
				FadeCanvasGroup(Fade.ToTransparent, gameplayCanvasGroup);
				ScaleMask(Scale.Down, levelFailedColour);
				yield return new WaitForSeconds(0.5F);
				spriteRenderer.color = levelFailedColour;
				FadeCanvasGroup(Fade.ToOpaque, levelFailedCanvasGroup);
				break;

			case ViewStates.LevelRestart:
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
				GameManager.Instance.playerPathFollower.enabled = false;
				tapToPlay.gameObject.SetActive(false);
				tapToPlayHitBox.gameObject.SetActive(false);
				tapToRestart.gameObject.SetActive(false);
				tapToRestartHitBox.gameObject.SetActive(false);
				settings.Button.parent.gameObject.SetActive(false);
				playerSelectionHitBox.gameObject.SetActive(false);
				ApplyPlayerSelectionUnlockableStates();
				previewCubeRotator.enabled = true;
				spriteRenderer.color = PlayerSelectColour;
				yield return ScaleMask(Scale.Down, PlayerSelectColour, 3.5F);
				yield return new WaitForSeconds(.05F);
				FadeCanvasGroup(Fade.ToOpaque, playerSelectCanvasGroup, 5F);
				yield return new WaitForSeconds(.05F);
				FadeSelectionItems(Fade.ToOpaque, 5F);
				break;
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

		float startAlpha = type == Fade.ToOpaque ? 0 : 1;
		float endAlpha	 = type == Fade.ToOpaque ? 1 : 0;

		while (t < 1)
		{
			t += Time.deltaTime * x;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
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
	/// Adjust the UI for the Player Skin unlockables currently unlocked by the player
	/// </summary>
	public void ApplyPlayerSelectionUnlockableStates()
	{
        for (int i = 0; i < playerUnlockables.Count; i++)
        {
			playerUnlockables[i].overlay.gameObject.SetActive(!GameSave.IsPlayerSkinUnlocked(i));
		}
	}

	/// <summary>
	/// Processes the Highlighting for the last and current touched UI Unlockable
	/// </summary>
	private bool HighlightOnUnlockableSelected(int index)
	{
		// If we pick the same Item, return
		if (index == skinIndex)
			return true;

		Debug.Log("Here 1");
		// If we have previous selected an unlockable and its not unlocked, set it to the locked colour
		if (skinIndex != -1 && !GameSave.IsPlayerSkinUnlocked(skinIndex))
			playerUnlockables[skinIndex].overlay.material = playerSelectLockedMat;

		skinIndex = index;

		Debug.Log("Here 2");
		// If we have already unlocked this skin, hide the purchase button, apply the skin and return
		if (GameSave.IsPlayerSkinUnlocked(skinIndex))
		{
			if (playerSelectionPurchaseButton.activeInHierarchy)
				playerSelectionPurchaseButton.SetActive(false);

			// Swap the current Materials
			SwapColoursOnSelection();
			return true;
		}

		Debug.Log("Here 3");
		// If we have not unlocked the skin, highlight the unlockable
		playerUnlockables[skinIndex].overlay.material = playerSelectHighlightMat;
		
		// If the purchase button is not visible, make it visible
		if (!playerSelectionPurchaseButton.activeInHierarchy)
			playerSelectionPurchaseButton.SetActive(true);

		// Set the cost of the unlockable and activate the dimmed overlay if we can't afford it
		playerSelectionPurchaseText.text = playerUnlockables[skinIndex].cost.ToString();
		playerSelectionPurchaseButtonOverlay.SetActive(GameSave.CoinCount < playerUnlockables[skinIndex].cost);

		return true;
	}

	/// <summary>
	/// Swaps the changeable colours when the Player selects a unlocked material
	/// </summary>
	private void SwapColoursOnSelection()
	{
		previewCubeMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.playerMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.PlayerTrailColour = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerParticleMaterial.color = LevelManager.Instance.GetOffsetColour(playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color, -100F);
	}

	/// <summary>
	/// Wrapper function for IFadePlayerSelectionItems
	/// </summary>
	private Coroutine FadeSelectionItems(Fade type, float fadeSpeed)
	{
		return StartCoroutine(IFadePlayerSelectionItems(type, fadeSpeed));
	}

	/// <summary>
	/// Fade the Selectable Player Cubes In or Out with speed
	/// </summary>
	private IEnumerator IFadePlayerSelectionItems(Fade type, float fadeSpeed)
	{
		float t = 0;

		Color[] fromColours = new Color[playerUnlockables.Count];

		for (int i = 0; i < fromColours.Length; i++)
		{
			fromColours[i] = new Color(playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.r,
									   playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.g,
									   playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.b, 
									   type == Fade.ToOpaque ? 0 : 1);
		}

		while (t < 1)
		{
			t += Time.deltaTime * fadeSpeed;
			for (int i = 0; i < playerUnlockables.Count; i++)
			{
				playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color = new Color(fromColours[i].r, fromColours[i].g, fromColours[i].b, type == Fade.ToOpaque ? t : 1F - t);

				if (type == Fade.ToTransparent)
				{
					if (playerUnlockables[i].cubeMeshRenderer.sharedMaterial.color.a <= 0)
						playerUnlockables[i].cubeMeshRenderer.gameObject.SetActive(false);
				}
				else
				{
					if (!playerUnlockables[i].cubeMeshRenderer.gameObject.activeInHierarchy)
						playerUnlockables[i].cubeMeshRenderer.gameObject.SetActive(true);
				}
			}
			previewCubeMeshRenderer.sharedMaterial.color = new Color(previewCubeMeshRenderer.sharedMaterial.color.r,
																	 previewCubeMeshRenderer.sharedMaterial.color.g,
																	 previewCubeMeshRenderer.sharedMaterial.color.b,
																	 type == Fade.ToOpaque ? t : 1F - t);

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
		levelProgressImage.fillAmount = distance / vertCount;
	}


	/// <summary>
	/// Update the UI Coins over time using the IUpdateUICoins Method
	/// </summary>
	/// <param name="oldCoinCount"></param>
	public void UpdateUICoins(float oldCoinCount)
	{
		StartCoroutine(IUpdateUICoins(oldCoinCount));
	}

	/// <summary>
	/// Update UI Coins over time
	/// </summary>
	private IEnumerator IUpdateUICoins(float oldCoinCount)
	{
		float sign = oldCoinCount < GameSave.CoinCount ? 1 : -1;

		// While the old coin coint is not equal to the new one, update our display value
		while ((int)oldCoinCount != GameSave.CoinCount)
		{
			oldCoinCount += Time.deltaTime * Instance.coinCounterUpdateSpeed * sign;
			Instance.coinCounterText.text = ((int)oldCoinCount).ToString();
			yield return null;
		}
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