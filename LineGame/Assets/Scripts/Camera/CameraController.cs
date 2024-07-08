using UnityEngine;

public class CameraController : Singleton<CameraController>
{
	// The point to pivot around
	[SerializeField] private Transform pivotPoint;

	// The offset position from the pivot point to follow
	[SerializeField] private Vector3 followOffset = Vector3.zero;

	// Height Offset of the pivot point
	[SerializeField] private float heightRotationOffset = 0;

	// rotational direction
	[SerializeField] private bool rotateClockwise = true;

	public float rotateSpeed = 1f;
	public float rotateRadiusX = 1f;
	public float rotateRadiusY = 1f;
	public float rotateRadiusZ = 1f;

	private float angle;

	[SerializeField] private bool followTarget;
	[SerializeField] private bool rotate;
	[SerializeField] private bool lookAtTarget;


	private void LateUpdate()
	{
		if (followTarget)
		{
			transform.position = pivotPoint.position + followOffset;

			if (angle != 0)
				angle = 0;
		}
		else if (rotate)
		{
			angle += (rotateClockwise ? 1 : -1) * rotateSpeed * Time.deltaTime;
			Vector3 offsetPosition = new Vector3(Mathf.Sin(angle) * rotateRadiusX, heightRotationOffset + (Mathf.Cos(angle) * rotateRadiusY), Mathf.Cos(angle) * rotateRadiusZ);
			transform.position = pivotPoint.position + offsetPosition;
		}
		
		if (lookAtTarget)
			transform.LookAt(pivotPoint);
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.DrawSphere(pivotPoint.position + new Vector3(0, heightRotationOffset, 0), 0.1f);
		Gizmos.DrawLine(transform.position, pivotPoint.position + new Vector3(0, heightRotationOffset, 0));
		Gizmos.DrawLine(transform.position, pivotPoint.position);
	}
#endif

	// Resets the camera for the next level
	public void ResetCamera()
	{
		followTarget = true;
		rotate = false;
		transform.position = pivotPoint.position + followOffset;
	}

	// Used when the player passes the finish line
	public void ToggleRotation()
	{
		followTarget = false;
		rotate = true;
	}
}