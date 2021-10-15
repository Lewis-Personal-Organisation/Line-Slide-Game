using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    #region AnimationCurves

    #endregion

    #region RectTransforms
    /// <summary>
    /// The Rightmost point of the RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float Width(this RectTransform _transform)
    {
        return _transform.sizeDelta.x;
    }

    /// <summary>
    /// The Half Width of the RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float HalfWidth(this RectTransform _transform)
    {
        return _transform.rect.size.x / 2F;
    }

    /// <summary>
    /// The Topmost point of the RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float Height(this RectTransform _transform)
    {
        return _transform.sizeDelta.y;
    }

    /// <summary>
    /// The Topmost point of the RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float HalfHeight(this RectTransform _transform)
    {
        return _transform.sizeDelta.y;
    }

    /// <summary>
    /// The Leftmost point of the RectTransform
    /// </summary>
    /// <param name="_transform"></param>
    /// <returns></returns>
    public static float Left(this RectTransform _transform)
    {
        return _transform.localPosition.x - _transform.HalfWidth();
    }

    public static float Right(this RectTransform _transform)
    {
        return _transform.localPosition.x + _transform.HalfWidth();
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

	//#region List<>
 //   public static float Sum(this List<InventoryItem> list)
	//{
 //       float x = 0;

	//	for (int item = 0; item < list.Count; item++)
	//	{
 //           x += list[item].weight;
	//	}

 //       return x;
	//}
	//#endregion
}
