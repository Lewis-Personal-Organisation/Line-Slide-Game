using System.Collections;
using UnityEngine;

public enum Canvas
{
    Left,
    Right,
    Top,
    Bottom,
    Center
}

public static class CanvasUtils
{
    #region RectTransforms
    #region Properties

    /// <summary>
    /// Finds the Vector2 Position for a given RectTransform and Canvas using predefined Canvas positions
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="canvasPosition"></param>
    /// <param name="canvasScale"></param>
    /// <param name="canvasRect"></param>
    /// <returns></returns>
    public static Vector2 GetPos(RectTransform rectTransform, Canvas canvasPosition, float canvasScale, RectTransform canvasRect)
	{
		switch (canvasPosition)
		{
			case Canvas.Left:
                return canvasRect.Left() - new Vector2(rectTransform.HalfWidth(canvasScale), 0);
			case Canvas.Right:
                return canvasRect.Right() + new Vector2(rectTransform.HalfWidth(canvasScale), 0);
			case Canvas.Top:
                return canvasRect.Top() + new Vector2(0, rectTransform.HalfHeight(canvasScale));
            case Canvas.Bottom:
                return canvasRect.Bottom() - new Vector2(0, rectTransform.HalfHeight(canvasScale));
            case Canvas.Center:
                return canvasRect.Center();
        }

        return Vector2.zero;
	}

    // NEW //
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
    #region Functionality
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

    public static void IsMoving(this RectTransform transform, bool option)
	{
        isMoving = option;
	}
    public static bool IsMoving(this RectTransform transform)
    {
        return isMoving;
    }

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
		float countingTimer = 0;

		while((countingTimer += (Time.deltaTime/time)) < 1F)
        {
            movingRect.position = Vector2.Lerp(fromPosition, toPosition, GameManager.instance.curveHelper.Evaluate(type, CurveMode.Out, countingTimer));
            //movingRect.position = Vector2.Lerp(fromPosition, toPosition, countingTimer);
            yield return null;
		}

        // Activate the moving item if its not already active
        if (!movingRect.gameObject.activeSelf)
            movingRect.gameObject.SetActive(true);

        // We are no longer moving
        isMoving = false;
    }

    ////////// CUSTOM MOVEMENTS
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

    public static Canvas RandomCanvasPos(int min, int max, int exclude)
    {
        Canvas pos = (Canvas)Random.Range(min, max);

        if ((int)pos == exclude)
            return RandomCanvasPos(min, max, exclude);
        else
            return pos;
    }
}
