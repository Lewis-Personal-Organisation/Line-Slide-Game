using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[ExecuteInEditMode]
public class ScaleToOrthoCamera : MonoBehaviour
{
	[SerializeField] private Camera mainCamera;
	[SerializeField] private SpriteRenderer spriteRenderer;

	private void Start()
	{
		spriteRenderer.FitToOrthoCamera(mainCamera);
	}
}