using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class TextBounce : MonoBehaviour
{
    public RectTransform textTransform;
    private Vector3 defaultScale = Vector3.zero;

    public float speed = 2;
    public float scaleMin;
    public float scaleMax;

    private bool isGrowing = false;

    private bool isEnabled;


    private void Awake()
    {
        defaultScale = textTransform.localScale;
    }

	private void OnEnable()
	{
        Bounce();
	}

    private void Update()
    {
        if (!isEnabled)
            return;

        if (isGrowing)
        {
            if (textTransform.localScale.x < scaleMax)
            {
                textTransform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 0) * speed;
            }
            else
            {
                textTransform.localScale = new Vector3(scaleMax, scaleMax, 0);
                isGrowing = false;
            }
        }
        else
        {
            if (textTransform.localScale.x >= scaleMin)
            {
                textTransform.localScale -= new Vector3(Time.deltaTime, Time.deltaTime, 0) * speed;
            }
            else
            {
                isGrowing = true;
                textTransform.localScale = new Vector3(scaleMin, scaleMin, 0);
            }
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
}
