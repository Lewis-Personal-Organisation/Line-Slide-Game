using PathCreation.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public CurveHelper curveHelper;

    public PathFollower pathfollower;

    public UnityEngine.Canvas uiCanvas;
    public RectTransform uiCanvasRectTransform;

    public bool gameplayEnabled = true;


    private void Awake()
    {
#if UNITY_EDITOR
		curveHelper.Generate();
#endif
		instance = this;
    }

	private void Start()
	{
        StartCoroutine(LevelManager.instance.LoadLevel(1));
	}

	private void Update()
    {
        if (!gameplayEnabled)
            return;

        pathfollower.OnUpdate();
	}

	private void LateUpdate()
    {
        CamFollow.instance.OnUpdate();
    }
}