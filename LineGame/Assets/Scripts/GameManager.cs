using PathCreation.Examples;
using RDG;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : Singleton<GameManager>
{
    public CurveHelper curveHelper;

    public PathFollower playerPathFollower;
	public Material playerParticleMaterial;

	public Camera mainCamera;
	public Canvas uiCanvas;
    public RectTransform uiCanvasRectTransform;

	public UniversalAdditionalCameraData additionalCameraData;
	public UniversalRenderPipelineAsset renderPipeline;


	[Header("TESTING/DEBUGGING")]
	public bool useTestLevel = false;
	public bool progressLevels = true;
	public bool AlwaysUseFirstLevel = false;
	public bool AlwaysResetCoins = false;
	public int forcedLevel = -1;


	new private void Awake()
    {
        base.Awake();
	}

	private void Start()
	{
		GameSave.ConfigureUnlocks();
		UIManager.Instance.ApplyPlayerSelectionUnlockableStates();

		Instance.playerPathFollower.SetPlayerControl(false);
		//Debug.Log($"Controllable: {false}");
		Vibration.enabled = PlayerPrefs.GetInt("vibrationState", 0) == 1 ? true : false;
		UIManager.Instance.settings.vibrateBackgroundImage.color = Vibration.enabled ? UIManager.Instance.settings.onColour : UIManager.Instance.settings.offColour;
		UIManager.Instance.settings.timerBackgroundImage.color = GameSave.LevelTimerEnabled ? UIManager.Instance.settings.onColour : UIManager.Instance.settings.offColour;
		UIManager.Instance.SetLevelTimerUIVisibility(GameSave.LevelTimerEnabled);

		playerPathFollower.CacheSplitCubePositions();
#if UNITY_EDITOR
		if (forcedLevel != -1)
		{
			LevelManager.Instance.LoadLevel(forcedLevel);
		}
		else if(useTestLevel)
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
#else
		LevelManager.Instance.LoadLevel(GameSave.CurrentLevel);
#endif
		if (AlwaysResetCoins)
		{
			GameSave.CoinCount = 0;
		}
		
		UIManager.Instance.coinCounterText.text = GameSave.CoinCount.ToString();
	}


	public void SetAntiAliasing(int option)
	{
		option = Mathf.Clamp(option, -1, 2);
		if (option == -1)
			additionalCameraData.antialiasing = AntialiasingMode.None;
		else
		{
			additionalCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
			additionalCameraData.antialiasingQuality = (AntialiasingQuality)option;
		}
	}
}