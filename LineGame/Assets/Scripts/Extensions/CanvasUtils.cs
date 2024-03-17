using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum CanvasPositions
{
    Left,
    Right,
    Top,
    Bottom,
    Center,
}

public static class CanvasUtils
{
	#region RectTransforms
	#region Properties

	/// <summary>
	/// Returns a specified UI position for the edge of the Canvas
    /// E.g, Using the Top position results in the RectTransform's bottom edge sitting on the Canvas' top edge
	/// </summary>
	/// <param name="rectTransform"></param>
	/// <param name="canvasPosition"></param>
	/// <param name="canvasScale"></param>
	/// <param name="canvasRect"></param>
	/// <returns></returns>
	public static Vector2 GetPos(RectTransform rectTransform, CanvasPositions canvasPosition, float canvasScale, RectTransform canvasRect, float widthPercent = 0, float heightPercent = 0)
	{
		switch (canvasPosition)
		{
			case CanvasPositions.Left:
                return canvasRect.Left() - new Vector2(rectTransform.HalfWidth(canvasScale), 0);
			case CanvasPositions.Right:
                return canvasRect.Right() + new Vector2(rectTransform.HalfWidth(canvasScale), 0);
			case CanvasPositions.Top:
                return canvasRect.Top() + new Vector2(0, rectTransform.HalfHeight(canvasScale));
            case CanvasPositions.Bottom:
                return canvasRect.Bottom() - new Vector2(0, rectTransform.HalfHeight(canvasScale));
            case CanvasPositions.Center:
                return canvasRect.Center();
		}

        return Vector2.zero;
	}

	/// <summary>
	/// Given a Screen Percentage, returns the equivalent position within a UI Canvas. Usefull for positioning elements programatically
	/// </summary>
	public static Vector2 GetPos(this RectTransform canvas, Vector2 screenPercent)
	{
		return new Vector2((canvas.rect.width * canvas.localScale.x / 100) * screenPercent.x,
						   (canvas.rect.height * canvas.localScale.y / 100) * screenPercent.y);
	}
	public static Vector2 Bottom(this RectTransform transform)
    {
        return new Vector2(transform.CanvasWidth()/2F, 0F);
    }
    public static Vector2 Top(this RectTransform transform)
    {
        return new Vector2(transform.CanvasWidth() / 2F, transform.CanvasHeight());
    }
    public static Vector2 Left(this RectTransform transform)
    {
        return new Vector2(0F, transform.CanvasHeight()/2F);
    }
    public static Vector2 Right(this RectTransform transform)
    {
        return new Vector2(transform.CanvasWidth(), transform.CanvasHeight() / 2F);
    }
    public static Vector2 BottomLeft(this RectTransform transform)
    {
        return Vector2.zero;
    }
    public static Vector2 BottomRight(this RectTransform transform)
	{
        return new Vector2(transform.CanvasWidth(), 0F);
	}
    public static Vector2 TopRight(this RectTransform transform)
    {
        return new Vector2(transform.CanvasWidth(), transform.CanvasHeight());
    }
    public static Vector2 TopLeft(this RectTransform transform)
    {
        return new Vector2(0F, transform.CanvasHeight());
    }
    public static Vector2 Center(this RectTransform transform)
	{
        return new Vector2(transform.CanvasWidth() / 2, transform.CanvasHeight() / 2F);
    }
    public static float CanvasWidth(this RectTransform transform)
	{
        return transform.rect.width * transform.localScale.x;
	}
    public static float CanvasHeight(this RectTransform transform)
    {
        return transform.rect.height * transform.localScale.y;
    }

    /// <summary>
    /// The Global Width of this RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float Width(this RectTransform _transform, float scaleFactor)
    {
        return _transform.rect.size.x  * scaleFactor;
    }

    /// <summary>
    /// The Global halfed Width of this RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float HalfWidth(this RectTransform _transform, float scaleFactor)
    {
        return (_transform.rect.size.x / 2F) * scaleFactor;
    }

    /// <summary>
    /// The Global Height of this RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float Height(this RectTransform _transform, float scaleFactor)
    {
        return _transform.rect.size.y * scaleFactor;
    }

    /// <summary>
    /// The Global halfed Height of this RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float HalfHeight(this RectTransform _transform, float scaleFactor)
    {
        return _transform.rect.size.y / 2F * scaleFactor;
    }

    #endregion
    #region Movement
    public enum Curves
    {
        ToDefaultPosition,
        ToDefaultPositionX,
        ToDefaultPositionY,
        LeftToCenter,
        RightToCenter,
        CenterToRight,
        ToMinusSelfHeight,
        Custom
    }
    public static bool isMoving = false;

    ////////// PREDEFINED MOVEMENTS
    public static void Move(this RectTransform _transform, MonoBehaviour mono, Vector2 fromPosition, Vector2 toPosition, float time, CurveType curve = CurveType.NONE)
	{
        if (isMoving)
		{
            Debug.LogWarning($"{_transform.name} is already moving. Wait before moving again", _transform.gameObject);
            return;
		}

        isMoving = true;
		mono.StartCoroutine(Move(_transform, fromPosition, toPosition, time, curve));
	}
    private static IEnumerator Move(RectTransform movingRect, Vector2 fromPosition, Vector2 toPosition, float time, CurveType type)
    {
		// Activate the moving item if its not already active
		if (!movingRect.gameObject.activeSelf)
			movingRect.gameObject.SetActive(true);

		float countingTimer = 0;

		while ((countingTimer += Time.deltaTime/time) < 1F)
        {
            movingRect.position = Vector2.Lerp(fromPosition, toPosition, GameManager.Instance.curveHelper.Evaluate(type, CurveMode.Out, countingTimer));
            yield return null;
		}

        // We are no longer moving
        isMoving = false;
    }
    public static void Move(this RectTransform _transform, float time, MonoBehaviour mono, Vector3 newPos, bool maintainParentOffset = false)
    {
        if (isMoving)
        {
            Debug.LogWarning($"{_transform.name} is already moving. Wait before moving again", _transform.gameObject);
            return;
        }

        isMoving = true;
        mono.StartCoroutine(Move(_transform, time, newPos, maintainParentOffset));
    }

    /// <summary>
    /// Allows movement of a RectTransform to a specific position over a period of time
    /// with the option of the movement maintaining the offset position from it's parent
    /// </summary>
    /// <param name="movingRect"></param>
    /// <param name="canvasRect"></param>
    /// <param name="time"></param>
    /// <param name="newPos"></param>
    /// <param name="maintainParentOffset"></param>
    /// <returns></returns>
    private static IEnumerator Move(RectTransform movingRect, float time, Vector3 newPos, bool maintainParentOffset = false)
    {
        // The rect we use as a parent to move our object
        RectTransform tempRect = null;
        bool isCanvasNull = movingRect.parent.GetComponent<UnityEngine.Canvas>() == null;

        if (maintainParentOffset == false && isCanvasNull)
        {
            // Create the temp RectTransform
            tempRect = new GameObject("TempMoveHelper", typeof(RectTransform)).GetComponent<RectTransform>();
            //Position the temp RectTransform in the heirarchy between our moving object and its parent
            tempRect.SetParent(movingRect.GetComponentInParent<UnityEngine.Canvas>().transform);
            tempRect.anchoredPosition = Vector2.zero;
            tempRect.SetParent(movingRect.parent);
            movingRect.SetParent(tempRect);
        }

        AnimationCurve curveX = new AnimationCurve(new Keyframe(0, movingRect.localPosition.x), new Keyframe(time, newPos.x));
        AnimationCurve curveY = new AnimationCurve(new Keyframe(0, movingRect.localPosition.y), new Keyframe(time, newPos.y));
        Vector3 perfectValue = new Vector3();
        float countingTimer = 0;

        perfectValue.Set(curveX.Evaluate(time), curveY.Evaluate(time), movingRect.localPosition.z);

        // Activate the moving item if its not already active
        if (!movingRect.gameObject.activeSelf)
            movingRect.gameObject.SetActive(true);

        // Evaluate timer until elapsed
        while ((countingTimer += Time.deltaTime) < time)
        {
            movingRect.localPosition = new Vector3(curveX.Evaluate(countingTimer), curveY.Evaluate(countingTimer), movingRect.localPosition.z);
            yield return null;
        }

        // The final transition step
        movingRect.localPosition = perfectValue;

        // Reassign our old parent
        if (maintainParentOffset == false && isCanvasNull)
        {
            movingRect.SetParent(tempRect.parent);
            UnityEngine.Object.Destroy(tempRect.gameObject);
        }

        // We are no longer moving
        isMoving = false;
    }

    #endregion
    #region Rotation
    /// <summary>
    /// Rotate a RectTransform over time providing an original and expected rotation over time
    /// </summary>
    /// <param name="Transform"></param>
    /// <param name="routineOwner"></param>
    /// <param name="fromRotation"></param>
    /// <param name="toRotation"></param>
    /// <param name="time"></param>
    public static void RotateOverTime(this Transform Transform, MonoBehaviour routineOwner, Vector3 fromRotation, Vector3 toRotation, float time, UnityAction onStart = null, UnityAction onDone = null)
	{
		routineOwner.StartCoroutine(IRotate(Transform, fromRotation, toRotation, time, onStart, onDone));
    }

	/// <summary>
	/// Rotate a RectTransform over time providing an additional rotation over time
	/// </summary>
	/// <param name="transform"></param>
	/// <param name="routineOwner"></param>
	/// <param name="additionalAngle"></param>
	/// <param name="time"></param>
	public static void RotateOverTime(this Transform transform, MonoBehaviour routineOwner, Vector3 additionalAngle, float time, UnityAction onStart = null, UnityAction onDone = null)
	{
		routineOwner.StartCoroutine(IRotate(transform, transform.eulerAngles, transform.eulerAngles + additionalAngle, time, onStart, onDone));
	}

	public static IEnumerator IRotate(Transform transform, Vector3 fromRotation, Vector3 toRotation, float time, UnityAction onStart, UnityAction onDone)
    {
		// Activate the moving item if its not already active
		if (!transform.gameObject.activeSelf)
			transform.gameObject.SetActive(true);

		float countingTimer = 0;

		while ((countingTimer += Time.deltaTime / time) < 1F)
		{
			transform.rotation = Quaternion.Euler(Vector3.Lerp(fromRotation, toRotation, countingTimer));
			yield return null;
		}

		transform.rotation = Quaternion.Euler(Vector3.Lerp(fromRotation, toRotation, 1));

		onDone?.Invoke();
	}
	#endregion
	#region Size
	public static void ResizeOverTime(this RectTransform transform, MonoBehaviour routineOwner, Vector3 extension, float time, UnityAction onStart = null, UnityAction onDone = null, float waitTime = 0)
	{
		routineOwner.StartCoroutine(IExtend(transform, extension, time, onStart, onDone, waitTime));
	}

	public static IEnumerator IExtend(RectTransform transform, Vector3 extension, float time, UnityAction onStart, UnityAction onDone, float waitTime)
	{
        yield return new WaitForSeconds(waitTime);

        onStart?.Invoke();

		float countingTimer = 0;

        Vector3 fromSize = transform.sizeDelta;
        extension += fromSize;

		while ((countingTimer += Time.deltaTime / time) < 1F)
		{
			transform.sizeDelta = Vector3.Lerp(fromSize, extension, countingTimer);
			yield return null;
		}

        transform.sizeDelta = Vector3.Lerp(fromSize, extension, 1);

		onDone?.Invoke();
	}

	public static void ResizeOverTimeWithActions(this RectTransform transform, MonoBehaviour routineOwner, Vector3 extension, float time, List<TimedAction> timedActions, float waitTime = 0)
	{
		routineOwner.StartCoroutine(IExtendActions(transform, extension, time, timedActions, waitTime));
	}

    /// <summary>
    /// The IEnumerated method for resizing a RectTransform. Use a list of TimedActions to call functionality at given times
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="extension"></param>
    /// <param name="time"></param>
    /// <param name="timedActions"></param>
    /// <param name="waitTime"></param>
    /// <returns></returns>
	public static IEnumerator IExtendActions(RectTransform transform, Vector3 extension, float time, List<TimedAction> timedActions, float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
        int actionIndex = 0;
		float countingTimer = 0;

		Vector3 fromSize = transform.sizeDelta;
		extension += fromSize;

		while (countingTimer < 1F)
		{
			countingTimer += (Time.deltaTime / time);

			transform.sizeDelta = Vector3.Lerp(fromSize, extension, countingTimer);

            //Debug.Log($"{countingTimer}/{timedActions[actionIndex].t}");

            if (countingTimer >= timedActions[actionIndex].timer)
			{
				//Debug.Log($"Calling action {actionIndex}/{timedActions.Count-1}");
				timedActions[actionIndex].action?.Invoke();
                if (actionIndex < timedActions.LastIndex())
                    actionIndex++;
			}

			yield return null;
		}
	}

    public struct TimedAction
    {
		// When criteria is met, should the action run once or multiple times?
		public enum Modes
        {
            Passive,
            Active,
        }

		public Modes mode;
		public float timer;
		public UnityAction action;

		/// <summary>
		/// Created a new Timed action with a Pass/Active mode, a time value to evaluate and action
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="time"></param>
		/// <param name="action"></param>
		public TimedAction(Modes mode, float time, UnityAction action)
        {
            if (action == null)
                Debug.LogError($"Action is null. Assign an action to use this class!");

            this.mode = mode;
            this.timer = time;
            this.action = action;
        }

        /// <summary>
        /// Evaluates whether the action should be invoked
        /// </summary>
        /// <param name="value"></param>
        public void Evaluate(float value)
        {
            if (action == null)
                return;

            if (value > this.timer)
			{
				action.Invoke();
                if (this.mode == Modes.Passive)
                {
                    action = null;
				}
			}
        }
    }
	#endregion

	#endregion
	#region Colour
	public static void FadeColour(this UnityEngine.UI.Image image, MonoBehaviour routineOwner, Color newColour, float time, UnityAction onStart = null, UnityAction onDone = null, float waitTime = 0)
	{
		routineOwner.StartCoroutine(IColourFade(image, newColour, time, onStart, onDone, waitTime));
	}

	public static IEnumerator IColourFade(UnityEngine.UI.Image image, Color newColour, float time, UnityAction onStart, UnityAction onDone, float waitTime = 0)
	{
        yield return new WaitForSeconds(waitTime);

		float countingTimer = 0;
		Color oldColour = image.color;

		while ((countingTimer += Time.deltaTime / time) < 1F)
		{
			image.color = Color32.Lerp(oldColour, newColour, countingTimer);
			yield return null;
		}

		image.color = Color.Lerp(oldColour, newColour, 1);
		onDone?.Invoke();
	}
	#endregion
	#region Camera
	public static float OrthographicHeight(this Camera _camera)
    {
        return _camera.orthographicSize * 2F;
    }

    public static float OrthographicWidth(this Camera _camera)
    {
        return (_camera.orthographicSize * 2) * (Screen.width / Screen.height);
    }
    #endregion

    public static CanvasPositions RandomCanvasPos(int min, int max, int exclude)
    {
        CanvasPositions pos = (CanvasPositions)UnityEngine.Random.Range(min, max);

        if ((int)pos == exclude)
            return RandomCanvasPos(min, max, exclude);
        else
            return pos;
    }

	/// <summary>
	/// Positive 2D (1,1,0)
	/// </summary>
	public static Vector3 Positive2D => Vector3.up + Vector3.right;
	//public static Vector3 Positive2D => new Vector3(1, 1, 0);



}
