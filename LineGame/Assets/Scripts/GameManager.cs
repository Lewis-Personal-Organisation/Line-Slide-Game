using PathCreation.Examples;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CurveHelper curveHelper;

    public PathFollower playerPathFollower;
	public Material playerParticleMaterial;

	public Camera mainCamera;
	public Canvas uiCanvas;
    public RectTransform uiCanvasRectTransform;

	[Header("TESTING/DEBUGGING")]
	public bool useTestLevel = false;
	public bool AlwaysUseFirstLevel = false;
	public bool AlwaysResetCoins = false;


	new private void Awake()
    {
        base.Awake();
	}

	private void Start()
	{
		GameSave.ConfigureUnlocks();
		UITouch.Instance.ApplyPlayerSelectionUnlockableStates();

		playerPathFollower.CacheSplitCubePositions();

		if (useTestLevel)
		{
			LevelManager.Instance.LoadLevel(-1);
		}
		else
		{
			if (AlwaysUseFirstLevel)
			{
				LevelManager.Instance.LoadLevel(1);
			}
			else
			{
				LevelManager.Instance.LoadLevel(GameSave.CurrentLevel);
			}
		}

		if (AlwaysResetCoins)
		{
			GameSave.CoinCount = 0;
		}
		
		UITouch.Instance.coinCounterText.text = GameSave.CoinCount.ToString();
	}
}