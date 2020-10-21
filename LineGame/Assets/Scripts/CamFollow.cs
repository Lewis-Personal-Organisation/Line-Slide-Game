using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public static CamFollow instance;

    public Transform player;

    public Vector3 distance;

    public bool isFollowing = true;

    public float jumpForce;

    private void Awake()
    {
        instance = this;
        distance = this.transform.position - player.transform.position;
        transform.LookAt(player.position);
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        if (!isFollowing)
            return;

        transform.position = player.transform.position + distance;
    }
}