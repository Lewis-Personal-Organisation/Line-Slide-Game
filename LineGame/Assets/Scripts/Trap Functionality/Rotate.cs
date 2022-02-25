using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 5;

    public enum moveTypes
    {
        Spin,
        Sine
    }

    public enum axis
	{
        X,
        Y,
        Z
	}

    public moveTypes type;
    public axis rotationAxis;


    private void Update()
    {
        if (transform == null)
            return;

        switch (type)
        {
            case moveTypes.Spin:
                if (rotationAxis == axis.X)
                    transform.Rotate(Vector3.right * speed * Time.deltaTime);
                else if (rotationAxis == axis.Y)
                    transform.Rotate(Vector3.up * speed * Time.deltaTime);
                else if (rotationAxis == axis.Z)
                    transform.Rotate(Vector3.forward * speed * Time.deltaTime);
                break;
            case moveTypes.Sine:
                transform.localPosition = new Vector3(0, Mathf.Sin(speed * Time.deltaTime), 0);
                break;
            default:
                break;
        }
    }
}
