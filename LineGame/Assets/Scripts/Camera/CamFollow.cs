using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CamFollow : Singleton<CamFollow>
{
    public Transform player;

    public Vector3 distance;
    public bool isFollowing = true;


	new private void Awake()
    {
        base.Awake();
    }

	private void LateUpdate()
	{
		if (isFollowing)
			transform.position = player.transform.position + distance;
	}
}