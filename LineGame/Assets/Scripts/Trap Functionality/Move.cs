using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
	public enum MoveType
	{
		PingPong,
	}
	public enum Axis
	{
		X,
		Y,
		Z
	}
	public enum MoveContext
	{
		Global,
		Local
	}

	public MoveType moveType = MoveType.PingPong;
	public Vector3 axis = Vector3.zero;
	public MoveContext moveContext = MoveContext.Global;
	public float speed;
	private Vector3 cachedPosition;


	private void Start()
	{
		cachedPosition = this.transform.position;
	}

	private void FixedUpdate()
	{
		switch (moveType)
		{
			case MoveType.PingPong:
				float value = Mathf.PingPong(Time.time * speed, 1);

				if (moveContext == MoveContext.Global)
					this.transform.position = cachedPosition + (axis * (value - 0.5F));
				else
					this.transform.localPosition = axis * value;
				break;
		}
	}
}
