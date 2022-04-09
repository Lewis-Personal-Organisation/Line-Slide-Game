using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
	public PathCreation.Examples.PathFollower pathFollower;
	public List<Level> levels = new List<Level>();
	public static int levelCount;

	public static LevelManager instance;

	public SlicedFilledImage image;
	public Vector2 onscreenPos;
	public Vector2 offscreenPos;


	private void Awake()
	{
		levelCount = levels.Count;
		instance = this;
	}

	private void Start()
	{
		offscreenPos = CanvasUtils.GetPos(image.rectTransform, Canvas.Top, UITouch.instance.canvas.scaleFactor, UITouch.instance.canvasTransform);
		onscreenPos = image.rectTransform.position;

		image.rectTransform.position = offscreenPos;
	}

	/// <summary>
	/// Loads a level given a level number. Sets the position to 0,0,0
	/// Note: Changes number to 0-based index
	/// </summary>
	public System.Collections.IEnumerator LoadLevel(int levelNum)
	{
		Level level = Instantiate(levels[levelNum - 1].gameObject, null).GetComponent<Level>();
		level.gameObject.transform.position = Vector3.zero;

		pathFollower.pathCreator = level.roadPathCreator;
		pathFollower.roadCreator = level.roadMeshCreator;
		WaterShaderAnimator.instance.meshRenderer = level.WaterMesh;

		pathFollower.transform.position = pathFollower.pathCreator.path.GetPointAtDistance(0, pathFollower.endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);

		// Trigger the Path and Road Mesh Creators manually, so they render on Build
		//pathFollower.roadCreator.ManualTriggeredUpdate();
		//level.pathMeshCreator.ManualTriggeredUpdate();

		yield return null;
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