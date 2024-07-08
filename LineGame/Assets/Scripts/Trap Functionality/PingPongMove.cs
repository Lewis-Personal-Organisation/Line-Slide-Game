using UnityEngine;

public class PingPongMove : MonoBehaviour
{
	[SerializeField] private Transform movingTransform;
    [SerializeField] private Transform A;
	[SerializeField] private Transform B;
    [SerializeField] private float speed;
	[SerializeField] private bool correctRotation = true;


	private void Start()
	{
		movingTransform.position = A.position;

		if (correctRotation)
			movingTransform.LookAt(B);
	}

	private void FixedUpdate()
	{
		movingTransform.position = Vector3.Lerp(A.position, B.position, Mathf.PingPong(Time.time * speed, 1));
	}
}