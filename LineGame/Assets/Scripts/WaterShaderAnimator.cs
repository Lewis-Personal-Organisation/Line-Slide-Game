using UnityEngine;

public class WaterShaderAnimator : MonoBehaviour
{
    public MeshRenderer renderer;

    public float waterHeight;
    public float moveSpeed;
    public float moveAggresiveness;

    private void Update()
    {
        renderer.sharedMaterial.SetFloat("_WaterHeight", waterHeight);
        renderer.sharedMaterial.SetVector("_MoveDirection", new Vector4(Mathf.Sin(Time.time * moveSpeed) * moveAggresiveness, 0, 0, 0));
    }
}
