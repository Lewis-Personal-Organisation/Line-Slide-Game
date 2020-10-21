using UnityEngine;

public class WaterShaderAnimator : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    [Space(20)]
    public float heightSpeed;
    public float heightAggresiveness;

    [Space(20)]
    public float moveSpeed;
    public float moveAggresiveness;

    private float optFloat = 0;


    private void Update()
    {
        optFloat = -.5F + Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness;
        meshRenderer.gameObject.transform.position = new Vector3(0F, optFloat, 0F);
        meshRenderer.sharedMaterial.SetFloat("_WaterHeight", Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness);
        meshRenderer.sharedMaterial.SetVector("_MoveDirection", new Vector4(Mathf.Sin(Time.time * moveSpeed) * moveAggresiveness, 0, 0, 0));
    }
}