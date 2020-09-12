using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class TextBounce : MonoBehaviour
{
    public RectTransform textTransform;
    private Vector3 defaultScale = Vector3.zero;

    public const float speed = 2;
    public float scaleMin;
    public float scaleMax;

    private bool isBouncing = false;
    private bool isEnlarging = false;


    private void Awake()
    {
        defaultScale = textTransform.localScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleBounce(true, 1);
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ToggleBounce(false);
        }
    }

    public void ToggleBounce(bool _choice, float _speed = 2)
    {
        if (_choice == false)
        {
            StopAllCoroutines();
            isBouncing = false;
            textTransform.localScale = defaultScale;
            return;
        }

        if (_choice == true)
        {
            if (isBouncing)
                return;

            textTransform.localScale = defaultScale;
            isBouncing = true;
            StartCoroutine(Bounce(_choice, _speed));
        }
    }

    public IEnumerator Bounce(bool _choice, float _speed)
    {
        while (isBouncing)
        {
            if (isEnlarging)
            {
                if (textTransform.localScale.x < scaleMax)
                {
                    textTransform.localScale += new Vector3(Time.deltaTime * _speed, Time.deltaTime * _speed, 0);
                }
                else
                {
                    textTransform.localScale = new Vector3(scaleMax, scaleMax, 0);
                    isEnlarging = false;
                }
            }
            else
            {
                if (textTransform.localScale.x >= scaleMin)
                {
                    textTransform.localScale -= new Vector3(Time.deltaTime * _speed, Time.deltaTime * _speed, 0);
                }
                else
                {
                    isEnlarging = true;
                    textTransform.localScale = new Vector3(scaleMin, scaleMin, 0);
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
