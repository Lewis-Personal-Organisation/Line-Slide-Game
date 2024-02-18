using PathCreation.Examples;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CurveHelper curveHelper;

    public PathFollower playerPathFollower;
	public Material playerParticleMaterial;

	public Canvas uiCanvas;
    public RectTransform uiCanvasRectTransform;

    public bool gameplayEnabled = true;
	public bool useTestLevel = false;


	new private void Awake()
    {
        base.Awake();
	}

	private void Start()
	{
		playerPathFollower.CacheSplitCubePositions();
		LevelManager.Instance.LoadLevel(useTestLevel ? -1 : GameSave.CurrentLevel);
	}
}