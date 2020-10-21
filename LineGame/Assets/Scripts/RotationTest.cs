using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    public Rigidbody rb;

    public float Speed;

    public Vector3 axis = new Vector3(1, 1, 0);
    public Vector3 clamp = new Vector3(5, 5, 5);

    public Space space;


    private void Update()
    {
        transform.Rotate(ClampedRotation(axis), Speed * Time.deltaTime, space);
        if (Input.GetKeyDown(KeyCode.Return))
        {
            rb.AddForce(new Vector3(100f, 100f, 100f), ForceMode.Acceleration);
        }
    }

    public Vector3 ClampedRotation(Vector3 _axis)
    {
        _axis = new Vector3(Mathf.Clamp(_axis.x, 0, clamp.x), Mathf.Clamp(_axis.y, 0, clamp.y), 0F)
        {
            z = 0
        };
        return _axis;
    }
}
