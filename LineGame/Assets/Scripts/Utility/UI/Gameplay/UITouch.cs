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


public class UITouch : Singleton<UITouch>
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
		public Transform Button;
		public GameObject slidingObject;
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


	new private void Awake()
	{
		base.Awake();

		FPSDispay.Instance.enabled = false;
		vSyncText.gameObject.SetActive(false);

		LinkUIObjectActions();

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
            if (touchingOverFrames)
                return;

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

			// TEST
			for (int i = 0; i < hitResults.Count; i++)
			{
				bool isValid = InstanceIDtoAction.ContainsKey(hitResults[i].gameObject.transform.GetInstanceID());

				if (isValid)
				{
					InstanceIDtoAction[hitResults[i].gameObject.transform.GetInstanceID()].Invoke();

					if (isTouchingUIElement)
						break;
				}
				else
				{
					Debug.Log(Utils.ColourText($"UI TEST. NO KEY FOR ID {hitResults[i].gameObject.transform.GetInstanceID()} ({hitResults[i].gameObject.name}).", Color.green));
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
	/// Assigns the touchable/interactable Objects to custom functionality
	/// InstanceID's have to be set using the Transform Component of an Object
	/// </summary>
	private void LinkUIObjectActions()
	{
		InstanceIDtoAction.Add(settings.Button.GetInstanceID(), () =>
			{
				if (!settings.isAnimating)
				{
					if (settings.open == false)
						settings.slidingObject.SetActive(true);

					settings.isAnimating = true;
					settings.Button.RotateOverTime(this, Vector3.forward * (settings.open ? 1F : -1F) * 60F, .3F);

					List<CanvasUtils.TimedAction> actions = new List<CanvasUtils.TimedAction>()
					{
						new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Passive, settings.open ? 0.2F: 0.7F, () =>
						{
							settings.testImage.rectTransform.ResizeOverTime(this, CanvasUtils.Positive2D * (settings.open ? 1F : -1F) * 40F, .1F, null);
						}),
						new CanvasUtils.TimedAction(CanvasUtils.TimedAction.Modes.Passive, 1F, () =>
						{
							settings.isAnimating = false;
							if (settings.open == false)
								settings.slidingObject.SetActive(false);
						})
					};

					settings.backgroundImage.rectTransform.ResizeOverTimeWithActions(this, (settings.open ? -1F : 1F) * settings.backgroundHeight * Vector3.up, .3F, actions);
					settings.open = !settings.open;
					Debug.Log("Settings Test!"); 
					isTouchingUIElement = true;
				}
			});
		InstanceIDtoAction.Add(tapToPlayHitBox.GetInstanceID(), () => {
			tapToPlay.gameObject.SetActive(false);
			tapToPlayHitBox.gameObject.SetActive(false);
			Instance.levelProgressImage.rectTransform.Move(this, Instance.progressImageOffScreenPos, Instance.progressImageOnScreenPos, 1.2F, CurveType.Exponential);
			playerSelectionHitBox.gameObject.SetActive(false);
			GameManager.Instance.playerPathFollower.SetTrailDistance();
			isTouchingUIElement = false;
		});
		InstanceIDtoAction.Add(tapToRestartHitBox.GetInstanceID(), () => {
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
			UpdateUICoins(oldCoinCount);
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
					Debug.Log("Stop Called");
					LevelManager.Instance.ToggleLevel(true);
					FadeSelectionCubes(Fade.ToTransparent, 5F);
					FadeOverlays(Fade.ToTransparent, 5F);
					yield return new WaitForSeconds(.05F);
					yield return FadeCanvasGroup(Fade.ToTransparent, playerSelectCanvasGroup, 2F);
					yield return new WaitForSeconds(.05F);
					yield return ScaleMask(Scale.Up, Color.black, 3.5F);
					StopCoroutine(playerSelectScrollCoroutine);
					SetPlayerSelectionObjectVisibility(false);
					GameManager.Instance.playerPathFollower.enabled = true;
					tapToPlay.gameObject.SetActive(true);
					tapToPlayHitBox.gameObject.SetActive(true);
					tapToRestart.gameObject.SetActive(true);
					tapToRestartHitBox.gameObject.SetActive(true);
					settings.Button.parent.gameObject.SetActive(true);
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
				break;

			case ViewStates.LevelComplete:
				ScaleMask(Scale.Down, Color.black);
				yield return new WaitForSeconds(0.5F);
				spriteRenderer.color = Color.black;
				yield return FadeCanvasGroup(Fade.ToTransparent, gameplayCanvasGroup);
				levelProgressImage.rectTransform.position = progressImageOffScreenPos;
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
				playerSelectScrollCoroutine = StartCoroutine(ScrollUIImage());
				GameManager.Instance.playerPathFollower.enabled = false;
				tapToPlay.gameObject.SetActive(false);
				tapToPlayHitBox.gameObject.SetActive(false);
				tapToRestart.gameObject.SetActive(false);
				tapToRestartHitBox.gameObject.SetActive(false);
				settings.Button.parent.gameObject.SetActive(false);
				playerSelectionHitBox.gameObject.SetActive(false);
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
			Vector2 sd = playerUnlockables[index].selectableImage.rectTransform.sizeDelta;	// Cache the og size

			// Scale down, and scale up once complete
			playerUnlockables[index].selectableImage.rectTransform.ResizeOverTime(this, sd * CanvasUtils.Negative2D * 0.20F, .075F, null, () =>
			{
				playerUnlockables[index].selectableImage.rectTransform.ResizeOverTime(this, sd * CanvasUtils.Positive2D * 0.20F, .075F, null, () =>
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
			return true;
		}

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
	private void SwapPlayerColoursOnSelection()
	{
		previewCubeMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.playerMeshRenderer.sharedMaterial.color = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerPathFollower.PlayerTrailColour = playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color;
		GameManager.Instance.playerParticleMaterial.color = LevelManager.Instance.GetOffsetColour(playerUnlockables[skinIndex].cubeMeshRenderer.sharedMaterial.color, -100F);
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

	private IEnumerator ScrollUIImage()
	{
		Debug.Log("Start Called");
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
			Debug.Log("Scrolling");
			yield return null;
		}
	}
}