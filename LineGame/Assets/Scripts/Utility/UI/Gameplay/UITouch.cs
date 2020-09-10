using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UITouch : MonoBehaviour
{
    public static UITouch instance;

    public enum TouchFilters
    {
        VSync,
        Settings,
        FPSCounterToggle,
        PathFollowerToggle,
        PathFollowerMaxSpeed,
        PathFollowerAccel,
    }

    private Dictionary<int, TouchFilters> InstanceIDtoFilter = new Dictionary<int, TouchFilters>();

    [SerializeField] TextMeshProUGUI vSyncText;
    public TextMeshProUGUI maxMoveSpeed;
    public TextMeshProUGUI moveAcceleration;

    public Transform settingsButton;

    public GameObject panelWindow;

    public Toggle FPSCounterToggleObj;
    public Toggle PathFollowerToggleObj;

    // Our android keyboard
    private TouchScreenKeyboard keyboard;


    // Graphics raycasting
    [SerializeField] private GraphicRaycaster uiRaycaster;
    [SerializeField] private PointerEventData uiPointerEventData;
    [SerializeField] private EventSystem uiEventSystem;
    public List<RaycastResult> hitResults = new List<RaycastResult>();

    // Used to filter out touch spamming by holding down a touch
    public bool touchingOverFrames = false;


    private void Awake()
    {
        instance = this;
        vSyncText.text = $"vSync: {QualitySettings.vSyncCount}";
        maxMoveSpeed.text = $"Max Speed: {GameManager.instance.pathfollower.maxSpeed}";
        moveAcceleration.text = $"Acceleration: {GameManager.instance.pathfollower.accelerationMultiplier}";

        InstanceIDtoFilter.Add(vSyncText.transform.GetInstanceID(), TouchFilters.VSync);
        InstanceIDtoFilter.Add(settingsButton.GetInstanceID(), TouchFilters.Settings);
        InstanceIDtoFilter.Add(FPSCounterToggleObj.transform.GetChild(1).transform.GetInstanceID(), TouchFilters.FPSCounterToggle);
        InstanceIDtoFilter.Add(PathFollowerToggleObj.transform.GetChild(1).transform.GetInstanceID(), TouchFilters.PathFollowerToggle);
        InstanceIDtoFilter.Add(maxMoveSpeed.transform.GetInstanceID(), TouchFilters.PathFollowerMaxSpeed);
        InstanceIDtoFilter.Add(moveAcceleration.transform.GetInstanceID(), TouchFilters.PathFollowerAccel);
    }


    // Each frame, if we have provided screen input, and aren't touching over multiple frame, register where the touch was located on screen
    // Store all hit objects hit, in our raycast list
    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) || Input.touchCount > 0)
        {
            // If we touched the screen in previous update, don't accept input over multiple frames
            if (touchingOverFrames)
                return;

            // If we didn't touch screen in last update, with this touch, we are now touching over previous frames
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
            // By default, the list is sorted from first to last from 0. E.g, Items found beneath are at the end of the array
            for (int i = 0; i < hitResults.Count; i++)
            {
                if (TouchFilter(InstanceIDtoFilter[hitResults[i].gameObject.transform.GetInstanceID()]))
                    break;
            }
        }
        else
        {
            // If we don't take input, we can't be touching over frames
            touchingOverFrames = false;
        }
    }

    // Filter the appropriate action for the UI items we clicked
    private bool TouchFilter(TouchFilters _filter)
    {
        switch (_filter)
        {
            case TouchFilters.VSync:
            case TouchFilters.PathFollowerMaxSpeed:
            case TouchFilters.PathFollowerAccel:
                StartCoroutine(WaitForKeyboard(_filter));
                return true;

            case TouchFilters.Settings:
                panelWindow.SetActive(!panelWindow.activeSelf);
                GameManager.instance.gameplayEnabled = !panelWindow.activeSelf;
                return true;

            case TouchFilters.FPSCounterToggle:
                FPSCounterToggleObj.isOn = !FPSCounterToggleObj.isOn;
                FPSDIspay.instance.update = FPSCounterToggleObj.isOn;
                if (!FPSCounterToggleObj.isOn)
                    FPSDIspay.instance.StopAndHideCounter();
                return true;

            case TouchFilters.PathFollowerToggle:
                PathFollowerToggleObj.isOn = !PathFollowerToggleObj.isOn;
                GameManager.instance.pathfollower.DoFollow = PathFollowerToggleObj.isOn;
                GameManager.instance.pathfollower.ResetPath();
                return true;
        }

        return false;
    }

    // The method used to Open the Android Keyboard to edit settings
    public IEnumerator WaitForKeyboard(TouchFilters _filter)
    {
        // If not mobile, return
#if UNITY_EDITOR
       yield return null;
#endif

        switch (_filter)
        {
            case TouchFilters.VSync:
                keyboard = TouchScreenKeyboard.Open(QualitySettings.vSyncCount.ToString(), TouchScreenKeyboardType.NumberPad);
                break;

            case TouchFilters.PathFollowerMaxSpeed:
                keyboard = TouchScreenKeyboard.Open(GameManager.instance.pathfollower.maxSpeed.ToString(), TouchScreenKeyboardType.NumberPad);
                break;

            case TouchFilters.PathFollowerAccel:
                keyboard = TouchScreenKeyboard.Open(GameManager.instance.pathfollower.accelerationMultiplier.ToString(), TouchScreenKeyboardType.NumberPad);
                break;
        }

        // While the keyboard is visible, stop gameplay and wait
        while (keyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            GameManager.instance.gameplayEnabled = false;
            yield return new WaitForEndOfFrame();
        }

        if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            yield return null;
        }

        int _parsedNum = 0;

        switch (_filter)
        {
            case TouchFilters.VSync:
                if (int.TryParse(keyboard.text, out _parsedNum))
                {
                    if (keyboard.text == "0")
                    {
                        Application.targetFrameRate = int.MaxValue;
                    }

                    vSyncText.text = $"vSync: {keyboard.text}";

                    QualitySettings.vSyncCount = (_parsedNum);
                }
                break;

            case TouchFilters.PathFollowerMaxSpeed:

                if (int.TryParse(keyboard.text, out _parsedNum))
                {
                    GameManager.instance.pathfollower.maxSpeed = _parsedNum;
                    maxMoveSpeed.text = $"Max Speed: {GameManager.instance.pathfollower.maxSpeed}";
                }
                break;

            case TouchFilters.PathFollowerAccel:
                if (int.TryParse(keyboard.text, out _parsedNum))
                {
                    GameManager.instance.pathfollower.accelerationMultiplier = _parsedNum;
                    moveAcceleration.text = $"Acceleration: {GameManager.instance.pathfollower.accelerationMultiplier}";
                }
                break;
        }
        GameManager.instance.gameplayEnabled = true;
    }
}