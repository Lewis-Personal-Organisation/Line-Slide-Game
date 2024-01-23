using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 5;
	public Vector3 offset = Vector3.zero;
    public float timeOffset = 0f;

	public enum MoveTypes
    {
        Spin,
        SineY
    }

    public enum Axis
	{
        X,
        Y,
        Z
	}

    public MoveTypes type;
    public Axis rotationAxis;

	private void Start()
	{
		
	}

	private void FixedUpdate()
	{
        switch (type)
        {
            case MoveTypes.Spin:
                if (rotationAxis == Axis.X)
                    transform.Rotate(Vector3.right * speed * Time.deltaTime);
                else if (rotationAxis == Axis.Y)
                    transform.Rotate(Vector3.up * speed * Time.deltaTime);
                else if (rotationAxis == Axis.Z)
                    transform.Rotate(Vector3.forward * speed * Time.deltaTime);
                break;
            case MoveTypes.SineY:
                transform.position = new Vector3(transform.position.x, Mathf.Sin((Time.time + timeOffset) * speed), transform.position.z) + offset;
                break;
            default:
                break;
        }
    }
}
