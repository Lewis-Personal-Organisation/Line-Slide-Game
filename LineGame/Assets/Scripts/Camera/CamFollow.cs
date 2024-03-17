using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class CamFollow : Singleton<CamFollow>
{
    public Transform player;

    public Vector3 distance;
	private Quaternion gameplayRotation;
    public bool isFollowing = true;


	new private void Awake()
    {
        base.Awake();
    }

	private void LateUpdate()
	{
		if (isFollowing)
			transform.position = player.transform.position + distance;

		gameplayRotation = this.transform.rotation;
	}
}