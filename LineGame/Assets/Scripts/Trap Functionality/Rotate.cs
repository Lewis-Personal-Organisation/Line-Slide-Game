using UnityEngine;

public class Rotate : MonoBehaviour
{
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
	public enum AxisContext
	{
		Global,
		Local
	}

	public MoveTypes type;
	public Axis rotationAxis;
	public AxisContext axisContext = AxisContext.Global;
	public float speed = 5;
	public Vector3 offset = Vector3.zero;
    public float timeOffset = 0f;

	
	private void FixedUpdate()
	{
        switch (type)
        {
            case MoveTypes.Spin:
				if (rotationAxis == Axis.X)
					transform.Rotate(Vector3.right * speed * Time.deltaTime, axisContext == AxisContext.Global ? Space.World : Space.Self);
				else if (rotationAxis == Axis.Y)
					transform.Rotate(Vector3.up * speed * Time.deltaTime, axisContext == AxisContext.Global ? Space.World : Space.Self);
				else if (rotationAxis == Axis.Z)
					transform.Rotate(Vector3.forward * speed * Time.deltaTime, axisContext == AxisContext.Global ? Space.World : Space.Self);
				break;
            case MoveTypes.SineY:
                if (axisContext == AxisContext.Global)
                    transform.position = new Vector3(transform.position.x, Mathf.Sin((Time.time + timeOffset) * speed), transform.position.z) + offset;
				else
					transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Sin((Time.time + timeOffset) * speed), transform.localPosition.z) + offset;
				break;
            default:
                break;
        }
    }
}
