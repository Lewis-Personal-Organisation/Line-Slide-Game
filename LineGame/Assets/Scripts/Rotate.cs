using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Transform transformToRotate;

    public float speed = 5;

    public enum moveTypes
    {
        Spin,
        Sine
    }

    public moveTypes type;

    private void Update()
    {
        if (transform == null)
            return;

        switch (type)
        {
            case moveTypes.Spin:
                transform.Rotate(Vector3.up * speed * Time.deltaTime);
                break;
            case moveTypes.Sine:
                transform.localPosition = new Vector3(0, Mathf.Sin(speed * Time.deltaTime), 0);
                break;
            default:
                break;
        }
    }
}
