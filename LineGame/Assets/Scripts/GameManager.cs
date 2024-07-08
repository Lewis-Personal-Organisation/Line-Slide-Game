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
		UITouch.Instance.ApplyPlayerSelectionUnlockableStates();

		Vibration.enabled = PlayerPrefs.GetInt("vibrationState", 0) == 1 ? true : false;
		UITouch.Instance.settings.vibrateBackgroundImage.color = Vibration.enabled ? UITouch.Instance.settings.vibrationOnColour : UITouch.Instance.settings.vibrationOffColour;

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
		
		UITouch.Instance.coinCounterText.text = GameSave.CoinCount.ToString();

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