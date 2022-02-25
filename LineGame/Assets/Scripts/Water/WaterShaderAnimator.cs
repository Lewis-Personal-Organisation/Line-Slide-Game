using UnityEngine;

public class WaterShaderAnimator : MonoBehaviour
{
    public enum WaterTypes
	{
        ConstantScroll,
        BackAndForth
	}

    public WaterTypes WaterType;

    [Multiline]
    public string _description = $"Animates our Water Shader attached to our Water Plane Gameobject using a Sin Wave";

    [Space(20)]
    public MeshRenderer meshRenderer;

    [Space(20)]
    public float heightSpeed;
    public float heightAggresiveness;

    [Space(20)]
    public float moveSpeed;
    public float moveAggresiveness;

    private float waterLevel = 0;


    private void Update()
    {
		switch (WaterType)
		{
			case WaterTypes.ConstantScroll:
				waterLevel = -0.5F + Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness;
				meshRenderer.gameObject.transform.position = new Vector3(0F, waterLevel, 0F);
				meshRenderer.sharedMaterial.SetFloat("_WaterHeight", Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness);
                meshRenderer.sharedMaterial.SetVector("_MoveDirection", new Vector4(Time.time * moveSpeed * moveAggresiveness, 0F, 0F, 0F));
                break;

			case WaterTypes.BackAndForth:
                waterLevel = -0.5F + Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness;
                meshRenderer.gameObject.transform.position = new Vector3(0F, waterLevel, 0F);
                meshRenderer.sharedMaterial.SetFloat("_WaterHeight", Mathf.Sin(Time.time * heightSpeed) * heightAggresiveness);
                meshRenderer.sharedMaterial.SetVector("_MoveDirection", new Vector4(Mathf.Sin(Time.time * moveSpeed) * moveAggresiveness, 0F, 0F, 0F));
                break;
		}
    }
}