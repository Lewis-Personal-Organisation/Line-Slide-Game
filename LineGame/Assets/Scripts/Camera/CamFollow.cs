using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class CamFollow : Singleton<CamFollow>
{
    public Transform player;

    public Vector3 distance;
    public bool isFollowing = true;
	//public Vector3 rotationPoint = Vector3.zero;
	public float radius = 0;
	public float speed = 0;
	private float step = 0;
	public float x = 0;
	public float z = 0;


	new private void Awake()
    {
        base.Awake();
    }

	private void LateUpdate()
	{
		if (isFollowing)
		{
			transform.position = player.transform.position + distance;
		}

		step = speed * Time.deltaTime;
		x = Mathf.Sin(step) * radius;
		//z = Mathf.Cos(step) * radius;
		transform.position = player.position + new Vector3(x, 0, 0);
		//transform.LookAt(player.transform);

		//transform.RotateAround(player.position, Vector3.up, 15 * Time.deltaTime);

		//Debug.DrawLine(this.transform.position, player.position);
	}
}