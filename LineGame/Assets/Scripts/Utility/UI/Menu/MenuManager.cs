using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Menu
{
	None,
	Test
}

public enum PositionCurves
{
	ToDefaultPositionX,
	ToDefaultPositionY,
	ToDefaultPositionXY,
	LeftToCenter,
	RightToCenter,
	CenterToRight,
	ToMinusSelfHeight,
}

public enum ScaleCurves
{
	ExpandFromCenterX,
	ExpandFromCenterY,
	ExpandFromCenterXY,
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

	public SlicedFilledImage levelProgressImage;

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
	private void Awake()
	{
		instance = this;
		StartCoroutine(RealtimeMenuExecution());
	}

	private void Start()
	{
		//MoveMenu(Menu.Test, PositionCurves.LeftToCenter, .5F);
		//ScaleMenu(Menu.Test, ScaleCurves.ExpandFromCenterXY, .5F, Vector3.zero, Vector3.one);
	}

	/// <summary>
	/// Moves a Menu Item in a specific direction, over a specified period of time using an Animation Curve.
	/// Transition is position perfect and scales with any Screen Size.
	/// </summary>
	/// <param name="_menu"></param>
	/// <param name="_curve"></param>
	/// <param name="_time"></param>
	/// <param name="_additionalUnits"></param>
	public void MoveMenu(Menu _menu, PositionCurves _curve, float _time, float _additionalUnits = 0)
	{
		if (_curve == PositionCurves.ToDefaultPositionXY)
		{
			menuStack.Enqueue(MoveMenuExpermintal(_menu, PositionCurves.ToDefaultPositionX, _time, _additionalUnits, true));
			menuStack.Enqueue(MoveMenuExpermintal(_menu, PositionCurves.ToDefaultPositionY, _time, _additionalUnits, true));
		}
		else
		{
			menuStack.Enqueue(MoveMenuExpermintal(_menu, _curve, _time, _additionalUnits, false));
		}
	}

	private IEnumerator MoveMenuExpermintal(Menu _menu, PositionCurves _direction, float _time, float _additionalUnits, bool _simultaniousExec = false)
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
			case PositionCurves.ToDefaultPositionX:
				_curve = CreateCurve(0, menus[_menu].item.localPosition.x, _time, menus[_menu].defaultPosition.x);
				perfectValue.Set(_curve.Evaluate(_time), menus[_menu].defaultPosition.y, menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.ToDefaultPositionY:
				_curve = CreateCurve(0, menus[_menu].item.localPosition.y, _time, menus[_menu].defaultPosition.y);
				perfectValue = new Vector3(menus[_menu].defaultPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.ToDefaultPositionXY:
				_curve = CreateCurve(0, menus[_menu].item.localPosition.y, _time, menus[_menu].defaultPosition.y);
				perfectValue = new Vector3(menus[_menu].defaultPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.LeftToCenter:
				_curve = CreateCurve(0, -instance.canvasTransform.HalfWidth() + -menus[_menu].item.HalfWidth(), _time, 0);
				perfectValue = new Vector3(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.CenterToRight:
				_curve = CreateCurve(0, 0, _time, instance.canvasTransform.HalfWidth() + menus[_menu].item.HalfWidth());
				perfectValue.Set(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.RightToCenter:
				_curve = CreateCurve(0, instance.canvasTransform.HalfWidth() + menus[_menu].item.HalfWidth(), _time, 0);
				perfectValue.Set(_curve.Evaluate(_time), menus[_menu].item.localPosition.y, menus[_menu].item.localPosition.z);
				break;

			case PositionCurves.ToMinusSelfHeight:
				_curve = CreateCurve(0, menus[_menu].item.localPosition.y, _time, menus[_menu].item.localPosition.y - menus[_menu].item.rect.size.y - _additionalUnits);
				perfectValue = new Vector3(menus[_menu].item.localPosition.x, _curve.Evaluate(_time), menus[_menu].item.localPosition.z);
				break;
		}

		// While we're not finished counting, move our Menu item by evaluating our created curve
		while (_countingTimer < _time)
		{
			if (_direction == PositionCurves.ToDefaultPositionY ||
				_direction == PositionCurves.ToMinusSelfHeight)
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

	public void ScaleMenu(Menu _menu, ScaleCurves _curve, float _time, Vector3 _startScale, Vector3 _endScale)
	{
		menuStack.Enqueue(ScaleMenuExpermintal(_menu, _curve, _time, _startScale, _endScale, false));
	}

	private IEnumerator ScaleMenuExpermintal(Menu _menu, ScaleCurves _direction, float _time, Vector3 _startScale, Vector3 _endScale, bool _simultaniousExec = false)
	{
		// A bool to decide on how many axis (X, Y) we carry out movement
		instance.multipleExecution = _simultaniousExec;

		// The curve we use for our movement
		AnimationCurve _curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0, 0));

		// The final value we should apply to a movement - precalculated below
		Vector3 perfectValue = new Vector3();

		// The value to evaluate our curve
		float _countingTimer = 0;

		switch (_direction)
		{
			case ScaleCurves.ExpandFromCenterX:
				_curve = CreateCurve(0, 0, _time, 1);
				perfectValue = new Vector3(_endScale.x, menus[_menu].item.localScale.y, menus[_menu].item.localScale.z);
				break;

			case ScaleCurves.ExpandFromCenterY:
				_curve = CreateCurve(0, 0, _time, 1);
				perfectValue = new Vector3(menus[_menu].item.localScale.x, _endScale.y, menus[_menu].item.localScale.z);
				break;

			case ScaleCurves.ExpandFromCenterXY:
				_curve = CreateCurve(0, 0, _time, 1);
				perfectValue = new Vector3(_endScale.x, _endScale.y, menus[_menu].item.localScale.z);
				break;
		}

		while (_countingTimer < _time)
		{
			if (_direction == ScaleCurves.ExpandFromCenterXY)
			{
				menus[_menu].item.localScale = new Vector3(_curve.Evaluate(_countingTimer), _curve.Evaluate(_countingTimer), menus[_menu].item.localScale.z);
			}
			else if (_direction == ScaleCurves.ExpandFromCenterX)
			{
				menus[_menu].item.localScale = new Vector3(_curve.Evaluate(_countingTimer), menus[_menu].item.localScale.y, menus[_menu].item.localScale.z);
			}
			else if (_direction == ScaleCurves.ExpandFromCenterY)
			{
				menus[_menu].item.localScale = new Vector3(menus[_menu].item.localScale.x, _curve.Evaluate(_countingTimer), menus[_menu].item.localScale.z);
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


	//UNTESTED
	public IEnumerator MenuScaleExperimental(Menu menu, ScalePoint point, float scalePercent, float time)
	{
		// Which axis are we resizing?
		RectTransform.Axis chosenAxis = point == ScalePoint.Left || point == ScalePoint.Right ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical;

		// The base value before we manipulate the transform. We use this to append the other values
		float baseValue = chosenAxis == RectTransform.Axis.Horizontal ? menus[menu].item.rect.size.x : menus[menu].item.rect.size.y;

		// The ultimate size value we want to append on completion, either X or Y axis
		float appendingSizeValue = (chosenAxis == RectTransform.Axis.Horizontal ? menus[menu].item.rect.size.x : menus[menu].item.rect.size.y) / 100 * scalePercent;

		// The ultimate pos value we want to append on completion, either X or Y axis
		Vector2 appendingPosValue = (chosenAxis == RectTransform.Axis.Horizontal ? Vector2.right : Vector2.up) * (appendingSizeValue / 2) * (point == ScalePoint.Left || point == ScalePoint.Bottom ? -1 : 1);

		float _elapsedTime = 0F;

		while (_elapsedTime < time)
		{
			menus[menu].item.SetSizeWithCurrentAnchors(chosenAxis, baseValue + (appendingSizeValue * (_elapsedTime / time)));
			menus[menu].item.anchoredPosition = (appendingPosValue * (_elapsedTime / time));
			_elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
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
					try
					{
						StartCoroutine(menuStack.Dequeue());
					}
					catch
					{
						Debug.LogError("We signaled Simultaneous Execution without needing it!");
					}
			}

			if (multiExecutionCount == 2)
			{
				multiExecutionCount = 0;
				routineRunning = false;
				multipleExecution = false;
			}

			yield return null;
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

	public void UpdateLevelProgress(float _distanceTravelled, float pathLength)
	{
		levelProgressImage.fillAmount = _distanceTravelled / pathLength;
	}
}