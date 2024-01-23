using PathCreation.Examples;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CurveHelper curveHelper;

    public PathFollower playerPathFollower;

    public UnityEngine.Canvas uiCanvas;
    public RectTransform uiCanvasRectTransform;

    public bool gameplayEnabled = true;


	new private void Awake()
    {
        base.Awake();
	}

	private void Start()
	{
		LevelManager.Instance.LoadLevel(GameSave.CurrentLevel);
	}

	private void Update()
	{

	}
}