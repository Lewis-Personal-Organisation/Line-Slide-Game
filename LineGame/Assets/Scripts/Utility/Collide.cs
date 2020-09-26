using UnityEngine;

public class Collide : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"We touched {collision.collider.gameObject.name}");
    }
}