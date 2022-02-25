using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class TextBounce : MonoBehaviour
{
    public RectTransform textTransform;

    public float duration = 2;
    public float scaleMin;
    public float scaleMax;

    public bool isGrowing = true;

    private bool isEnabled;

    [SerializeField]
    private float timer;


    private void OnEnable()
    {
        ResetValues();
        Bounce();
    }

    // Each frame, If we are enabled:
    // In/Decrement our timer based on grow or shrink
    // Using our timer, find the scale between our min and max scale
    // If our timer reaches an end point, reset it and invert our grow/shrink instruction
    private void Update()
    {
        if (!isEnabled)
            return;

        timer += ((isGrowing ? Time.deltaTime : -Time.deltaTime) / duration);
        textTransform.localScale = new Vector3(Mathf.Lerp(scaleMin, scaleMax, timer), Mathf.Lerp(scaleMin, scaleMax, timer), 0);

        if (timer >= 1F || timer <= 0F)
		{
			isGrowing = !isGrowing;
			timer = isGrowing ? 0 : 1;
		}
    }

    public void Stop()
    {
        isEnabled = false;
    }

    public void Bounce()
    {
        isEnabled = true;
    }

    public void ResetValues()
    {
        timer = 0;
        this.transform.localScale = Vector3.one;
        isGrowing = true;
    }
}

