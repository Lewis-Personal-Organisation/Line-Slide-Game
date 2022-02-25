using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum Menu
{
    None,
    Test
}

public enum Curves
{
    ToDefaultPosition,
    ToDefaultPositionX,
    ToDefaultPositionY,
    LeftToCenter,
    RightToCenter,
    CenterToRight,
    ToMinusSelfHeight,
}

public enum ScalePoint
{
    Top,
    Bottom,
    Left,
    Right,
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public IMenu[] menuList;       // The Menu RectTransforms

    public Dictionary<Menu, IMenu> menus = new Dictionary<Menu, IMenu>();      // A Dictionary for linking

    private static Queue<IEnumerator> menuStack = new Queue<IEnumerator>();

    private int multiExecutionCount = 0;
    private bool multipleExecution = false;
    private bool routineRunning = false;

    private static bool menusAreQueued => menuStack.Count > 0;

    public RectTransform canvasTransform;
    public UnityEngine.Canvas menuCanvas;

    /// <summary>
    /// <para> Gets the Current Canvas Width and Height</para> <para> SHOULD NOT BE CALLED IN AWAKE()</para>
    /// </summary>
    public Vector2 ActiveCanvasSize
    {
        get
        {
            return instance.canvasTransform.sizeDelta;
        }
    }

    // Sets up this class
    void Awake()
    {
        instance = this;
        StartCoroutine(RealtimeMenuExecution());
    }

    private void Start()
    {
        //MoveMenu(Menu.Test, Curves.ToMinusSelfHeight, 2F, 0F);
    }


    /// <summary>
    /// Moves a Menu Item in a specific direction, over a specified period of time using an Animation Curve.
    /// Transition is position perfect and scales with any Screen Size.
    /// </summary>
    /// <param name="_menu"></param>
    /// <param name="_curve"></param>
    /// <param name="_time"></param>
    /// <param name="_additionalUnits"></param>
    public void MoveMenu(Menu _menu, Curves _curve, float _time, float _additionalUnits = 0)
    {
        if (_curve == Curves.ToDefaultPosition)
        {
            menuStack.Enqueue(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionX, _time, _additionalUnits, true));
            menuStack.Enqueue(MoveMenuExpermintal(_menu, Curves.ToDefaultPositionY, _time, _additionalUnits, true));

        }
        else
        {
            menuStack.Enqueue(MoveMenuExpermintal(_menu, _curve, _time, _additionalUnits, false));
        }
    }

    private IEnumerator MoveMenuExpermintal(Menu _menu, Curves _direction, float _time, float _additionalUnits, bool _simultaniousExec = false)
    {
        // A bool to decide on how many axis (X, Y) we carry out movement
        instance.multipleExecution = _simultaniousExec;

        // The curve we use for our movement
        AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0));

        // The final value we should apply to a movement - precalculated below
        Vector3 perfectValue = new Vector3();

        // The value to evaluate our curve
        float _countingTimer = 0;

        // Create the animation curve for our type of movement
        switch (_direction)
        {
            case Curves.ToDefaultPositionX:
                _curve = CreateCurve(0, menus[_menu].item.localPosition.x, _time, menus[_menu].defaultPosition.x);
                perfectValue.Set(_curve.Evaluate(_time), menus[_menu].defaultPosition.y, menus[_menu].item.localPosition.z);
                break;

            case Curves.ToDefaultPositionY:
                _curve = CreateCurve(0, menus[_menu].item.localPosition.y, _time, menus[_menu].defaultPosition.y);
                perfectValue = new Vector3(menus[_menu].defaultPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
                break;

            case Curves.LeftToCenter:
                //_curve = CreateCurve(0, -instance.canvasTransform.HalfWidth() + -menus[_menu].item.HalfWidth(), _time, 0);
                perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                break;

            case Curves.CenterToRight:
                //_curve = CreateCurve(0, 0, _time, instance.canvasTransform.HalfWidth() + menus[_menu].item.HalfWidth());
                perfectValue.Set(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                break;

            case Curves.RightToCenter:
                //_curve = CreateCurve(0, instance.canvasTransform.HalfWidth() + menus[_menu].item.HalfWidth(), _time, 0);
                perfectValue.Set(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
                break;

            case Curves.ToMinusSelfHeight:
                _curve = CreateCurve(0, menus[_menu].item.localPosition.y, _time, menus[_menu].item.localPosition.y - menus[_menu].item.rect.size.y - _additionalUnits);
                perfectValue = new Vector3(menus[_menu].item.localPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
                break;
        }

        // While we're not finished counting, move our Menu item by evaluating our created curve
        while (_countingTimer < _time)
        {
            if (_direction == Curves.ToDefaultPositionY ||
                _direction == Curves.ToMinusSelfHeight)
            {
                menus[_menu].item.localPosition = new Vector3(menus[_menu].item.localPosition.x, _curve.Evaluate(_countingTimer), menus[_menu].item.localPosition.z);
            }
            else
            {
                menus[_menu].item.localPosition = new Vector3(_curve.Evaluate(_countingTimer), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
            }

            // Activate the Item if its not already active
            if (!menus[_menu].item.gameObject.activeSelf)
                menus[_menu].item.gameObject.SetActive(true);

            // Increment our timer
            _countingTimer += Time.deltaTime;
            yield return null;
        }

        // For the final transition step, place the item to its final position
        menus[_menu].item.localPosition = perfectValue;

        if (!_simultaniousExec)
            instance.routineRunning = false;

        if (instance.multipleExecution)
            instance.multiExecutionCount++;
    }

    // Resets our menu position to default position
    private void ResetMenuTransition(Menu _menu)
    {
        menus[_menu].item.localPosition = menus[_menu].defaultPosition;
    }

    public IEnumerator RealtimeMenuExecution()
    {
        while (true)
        {
            while (!menusAreQueued)
                yield return null;

            if (!routineRunning)
            {
                routineRunning = true;
                StartCoroutine(menuStack.Dequeue());

                if (multipleExecution)
                    StartCoroutine(menuStack.Dequeue());
            }

            if (multiExecutionCount == 2)
            {
                multiExecutionCount = 0;
                routineRunning = false;
                multipleExecution = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }


    /// <summary>
    /// Create a new Animation Curve with a Start and End value
    /// </summary>
    /// <param name="_curve"></param>
    /// <param name="_startTime"></param>
    /// <param name="_startValue"></param>
    /// <param name="_endTime"></param>
    /// <param name="_endValue"></param>
    public static AnimationCurve CreateCurve(float _startTime, float _startValue, float _endTime, float _endValue)
    {
        return new AnimationCurve(
            new Keyframe(_startTime, _startValue),
            new Keyframe(_endTime, _endValue)
            );
    }
}