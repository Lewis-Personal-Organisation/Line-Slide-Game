using TMPro;
using UnityEngine;

public class FPSDispay : MonoBehaviour
{
    public static FPSDispay instance;

    [SerializeField] private TextMeshProUGUI display;


    public static readonly int fixedStep = 1;
    private const float interval = 1;
    public static int frameCount = 0;

    public static float timer = 0;
    public int frameCap = -1;

    public bool update = true;


    private void Awake()
    {
        instance = this;
    }

    public void OnUpdate()
    {
        if (!update)
            return;

        timer += Time.deltaTime;
        frameCount++;

        if (timer >= interval)
            ResetCount();
    }
    public void ResetCount()
    {
        display.text = $"{frameCount}\n{(float)(1000f / frameCount)}ms";
        timer = 0;
        frameCount = 0;
    }

    public void StopAndHideCounter()
    {
        display.text = $"";
        timer = 0;
        frameCount = 0;
    }
}
