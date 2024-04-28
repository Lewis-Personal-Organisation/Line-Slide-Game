using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerUnlockable
{
	public Image selectableImage;
	public int cost;
	public MeshRenderer cubeMeshRenderer;
	public Image overlay;
	public bool isAnimating;
}
