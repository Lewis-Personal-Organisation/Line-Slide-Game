using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public static CamFollow instance;

    public Transform player;

    public Vector3 distance;
    public bool isFollowing = true;
    public bool isFocusing = true;


    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        if (isFocusing)
            transform.LookAt(player.position);

        if (isFollowing)
            transform.position = player.transform.position + distance;
    }
}