using TMPro;
using UnityEngine;

public class FPSDispay : Singleton<FPSDispay>
{

    [SerializeField] private TextMeshProUGUI display = null;

    public Timer frameTimer = null;

    public static int frameCount = 0;


	private void Awake()
	{
		base.Awake();
		frameTimer = new Timer();
		frameTimer.parent = this;
		frameTimer.SetName("FPSDisplay");
	}

	public void Show()
	{
		display.gameObject.SetActive(true);
		frameTimer.Begin(0,
			float.MaxValue, 1,
			UpdateFrameCount,
			UpdateUI);
	}

	private void OnDisable()
	{
		frameTimer.Reset();
	}

    private void UpdateFrameCount()
    {
        frameCount++;
    }

    public void UpdateUI()
    {
        display.text = $"{frameCount} | {(float)(1000f / frameCount)}ms";
        frameCount = 0;
    }

    public void StopAndHideCounter()
    {
        display.text = $"";
        frameCount = 0;
    }
}