using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Title: UITouch Graphics Raycast Manager
// Description: Allows graphics raycasting and attaching custom functionality to specific objects using their InstanceID() for a totally customisable system
// Objects are linked in Awake() to a Touch Filter and the filters functionality is then processed once the object is touched
// Objects can be linked for functionality but excluded from outside checks.
// Author: Lewis Dawson

public class UITouch : MonoBehaviour
{
    public static UITouch instance;

    public enum TouchFilters
    {
        Settings,
        SettingsWindow,
        PathFollowerToggle,
        DebugToggle,
        TapToPlay,
    }

    public Dictionary<int, TouchFilters> InstanceIDtoFilter = new Dictionary<int, TouchFilters>();

    // Graphics raycasting
    [Header("Critical UI Systems")]
    [SerializeField] private GraphicRaycaster uiRaycaster = null;
    [SerializeField] private PointerEventData uiPointerEventData = null;
    [SerializeField] private EventSystem uiEventSystem = null;
    private static List<RaycastResult> hitResults = new List<RaycastResult>();
    public UnityEngine.Canvas canvas;
    public RectTransform canvasTransform;

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI vSyncText = null;
    public Transform settingsButton;
    public Transform settingsWindow;
    public Toggle PathFollowerToggleObj;
    public TextBounce tapToPlay;
    public RectTransform tapToPlayHitBox;

    // Our android keyboard
    private TouchScreenKeyboard keyboard;
    public bool keyboardOpenRequested;


    // Used to filter out touch spamming by holding down a touch
    public bool touchingOverFrames = false;

    // Determines if we are touching a UI item
    public static bool isTouchingUIItem => hitResults.Count > 0;

    [SerializeField] private Timer debugTimer = null;


    private void Awake()
    {
        instance = this;

        vSyncText.text = $"vSync: {QualitySettings.vSyncCount}";

        debugTimer = new Timer();
        debugTimer.parent = this;
        debugTimer.SetName("UI Touch Debug Activator");

        FPSDispay.inst.ToggleVisibility(DebugActivator.instance.isActive);
        vSyncText.gameObject.SetActive(DebugActivator.instance.isActive);

        InstanceIDtoFilter.Add(settingsButton.GetInstanceID(), TouchFilters.Settings);
        InstanceIDtoFilter.Add(settingsWindow.GetInstanceID(), TouchFilters.SettingsWindow);
        InstanceIDtoFilter.Add(PathFollowerToggleObj.transform.GetChild(1).transform.GetInstanceID(), TouchFilters.PathFollowerToggle);
        InstanceIDtoFilter.Add(DebugActivator.instance.hiddenDebugControl.GetInstanceID(), TouchFilters.DebugToggle);
        InstanceIDtoFilter.Add(tapToPlayHitBox.GetInstanceID(), TouchFilters.TapToPlay);
    }


    // Each frame, if we have provided screen input, and aren't touching over multiple frames, register where the touch was located on screen
    // Store all hit objects hit, in our raycast list
    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) || Input.touchCount > 0)
        {
            // If we touched the screen in previous update, don't accept input over multiple frames
            if (touchingOverFrames)
                return;

            // If we didn't touch screen in last update, We are now touching over previous frames with this frame
            touchingOverFrames = true;

            //Set up the new Pointer Event
            uiPointerEventData = new PointerEventData(uiEventSystem);

            //Set the Pointer Event Position to that of the mouse position
            uiPointerEventData.position = Input.mousePosition;

            //Refresh our list of Raycast Results
            hitResults.Clear();

            //Raycast using the Graphics Raycaster and mouse click position
            uiRaycaster.Raycast(uiPointerEventData, hitResults);

            // Do the appropriate action for the hit object. We only want to hit the top-most item, so we break if we find a dictionary match.
            // By default, the list is sorted from first to last from index 0. E.g, Items found beneath are at the end of the array
            for (int i = 0; i < hitResults.Count; i++)
            {
                if (InstanceIDtoFilter.TryGetValue(hitResults[i].gameObject.transform.GetInstanceID(), out TouchFilters foundFilter))
                {
					if (ProcessFilter(foundFilter, i))
                        break;
                }
                else
                {
                    Debug.Log($"Raycast hit {i}: {hitResults[i].gameObject.name} is not registered as clickable");
                }
            }
        }
        else
        {
            touchingOverFrames = false;
        }
    }

    // Filter the appropriate action for the UI items we touched
    private bool ProcessFilter(TouchFilters _filter, int index)
    {
        switch (_filter)
        {
            case TouchFilters.Settings:
                settingsWindow.gameObject.SetActive(!settingsWindow.gameObject.activeSelf);
                GameManager.instance.gameplayEnabled = !settingsWindow.gameObject.activeSelf;
                return true;

            case TouchFilters.SettingsWindow:
                Debug.Log("Touched Settings Window");
                return true;

            case TouchFilters.PathFollowerToggle:
                PathFollowerToggleObj.isOn = !PathFollowerToggleObj.isOn;
                GameManager.instance.pathfollower.doFollow = PathFollowerToggleObj.isOn;
                GameManager.instance.pathfollower.ResetPath();
                return true;

            case TouchFilters.DebugToggle:
                // Start a timer. If we hold onto this object for 3 seconds, show FPS counter
                debugTimer.Begin(0, float.MaxValue, 3,
                    new UnityEngine.Events.UnityAction(delegate
                    {
                        if (!touchingOverFrames)
                        {
                            debugTimer.Restart();
                        }
                    }),
                    new UnityEngine.Events.UnityAction(delegate
                    {
                        DebugActivator.instance.isActive = !DebugActivator.instance.isActive;
                        FPSDispay.inst.update = DebugActivator.instance.isActive;
                        vSyncText.gameObject.SetActive(DebugActivator.instance.isActive);
                        FPSDispay.inst.ToggleVisibility(DebugActivator.instance.isActive);
                    }));
                return true;

            // When we touch the tap to play Hitbox, hide it and start movement
            case TouchFilters.TapToPlay:
                tapToPlay.gameObject.SetActive(false);
                tapToPlayHitBox.gameObject.SetActive(false);

                Vector2 startPos = CanvasUtils.GetPos(Levels.instance.image.rectTransform, Canvas.Top, canvas.scaleFactor, canvasTransform);
                Levels.instance.image.rectTransform.Move(this, startPos, Levels.instance.onscreenPos, 1.2F, CurveType.Exponential);

                // While we touch Tap-To-Play, we don't want to stop gameplay movement, so remove this item from the list immediately
                hitResults.RemoveAt(index);
                return false;
        }
        return false;
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