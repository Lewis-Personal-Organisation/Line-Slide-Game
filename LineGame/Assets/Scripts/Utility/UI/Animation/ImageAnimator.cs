using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour
{
	[SerializeField] private int frameRate;
	private int index = 0;
	private float time = 0;
	private float maxTime = 0;
	[SerializeField] private Image image;
	[SerializeField] private Sprite[] sprites;

	private void Start()
	{
		frameRate = Mathf.Clamp(frameRate, 24, Screen.currentResolution.refreshRate);
		maxTime = 1F / frameRate;
	}

	private void Update()
	{
		if ((time += Time.deltaTime) < maxTime)
			return;
	
        image.sprite = sprites[index];
		index++;
		time = 0;

		if (index > sprites.Length - 1)
		{
			index = 0;
		}
	}
}
