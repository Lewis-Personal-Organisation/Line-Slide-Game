using UnityEngine;

public class Levels : MonoBehaviour
{
	public static Levels instance;

	public SlicedFilledImage image;
	public Vector2 onscreenPos;
	public Vector2 offscreenPos;


	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		offscreenPos = CanvasUtils.GetPos(image.rectTransform, Canvas.Top, UITouch.instance.canvas.scaleFactor, UITouch.instance.canvasTransform);
		onscreenPos = image.rectTransform.position;

		image.rectTransform.position = offscreenPos;
	}

	/// <summary>
	/// Update our level progress image to match our travelled distance in the level
	/// </summary>
	/// <param name="distance"></param>
	/// <param name="vertCount"></param>
	public void UpdateUI(float distance, float vertCount)
	{
		image.fillAmount = distance / vertCount;
	}
}