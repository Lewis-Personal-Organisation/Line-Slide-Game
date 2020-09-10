using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public static CamFollow instance;

    public Transform player;
    public Vector3 distance;

    private void Awake()
    {
        instance = this;
        distance = this.transform.position - player.transform.position;
    }

    // Update is called once per frame
    public void OnUpdate()
    {
        transform.position = player.transform.position + distance;
    }
}
