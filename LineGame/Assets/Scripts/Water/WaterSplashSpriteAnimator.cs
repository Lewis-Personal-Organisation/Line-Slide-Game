using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaterSplashSpriteAnimator : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public Vector3 minSize, maxSize;
	public float speed;

	public float t = 0;
	public bool started = false;


	public void Animate()
	{
		started = true;
		StartCoroutine(WaterAnimation());
	}

	private IEnumerator WaterAnimation()
	{
		while (t < 1)
		{
			t += Time.deltaTime * speed;
			transform.localScale = Vector3.Lerp(minSize, maxSize, t);
			spriteRenderer.color = Color.Lerp(LevelManager.Instance.waterSpriteStartColour, LevelManager.Instance.waterSpriteEndColour, t);

			yield return null;
		}
	}
}
