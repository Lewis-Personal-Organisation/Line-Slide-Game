using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDIspay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;

    public static readonly int fixedStep = 1;
    public float interval = 1;
    public static  int frameCount = 0;

    public static float timer = 0;


    private void Update()
    {
        timer += Time.deltaTime;
        frameCount++;

        if (timer >= interval)
            ResetCount();
    }

    private void ResetCount()
    {
        display.text = $"FPS: {frameCount}";
        timer = 0;
        frameCount = 0;
    }
}
