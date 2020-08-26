using UnityEngine;

[ExecuteInEditMode]
public class TextBounce : MonoBehaviour
{
    public RectTransform textTransform;

    public float speed;
    public float scaleMin;
    public float scaleMax;

    public bool doBounce = false;
    public bool isEnlarging = false;
    
    

    void Update()
    {
        if (!doBounce)
            return;

        if (isEnlarging)
        {
            if (textTransform.localScale.x < scaleMax)
            {
                textTransform.localScale += new Vector3(Time.deltaTime * speed, Time.deltaTime * speed, 0);
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
                textTransform.localScale -= new Vector3(Time.deltaTime * speed, Time.deltaTime * speed, 0);
            }
            else
            {
                isEnlarging = true;
                textTransform.localScale = new Vector3(scaleMin, scaleMin, 0);
            }
        }
    }
}
