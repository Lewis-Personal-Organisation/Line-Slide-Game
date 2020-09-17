using TMPro;
using UnityEngine;

public class FPSDispay : MonoBehaviour
{
    public static FPSDispay instance;

    [SerializeField] private TextMeshProUGUI display = null;

    public Timer frameTimer;

    public static int frameCount = 0;

    public bool update = true;


    private void Awake()
    {
        instance = this;
        frameTimer.SetName("FPSDisplay");
    }

    private void UpdateFrameCount()
    {
        frameCount++;
    }

    public void UpdateUI()
    {
        display.text = $"{frameCount}\n{(float)(1000f / frameCount)}ms";
        frameCount = 0;
    }

    public void StopAndHideCounter()
    {
        display.text = $"";
        frameCount = 0;
    }

    public void ToggleVisibility(bool _choice)
    {
        display.gameObject.SetActive(_choice);

        if (_choice)
        {
            frameTimer.Begin(0,
            float.MaxValue, 1,
            UpdateFrameCount,
            UpdateUI);
        }
        else
        {
            frameTimer.Reset();
        }
    }
}