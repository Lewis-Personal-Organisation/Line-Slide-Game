using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PlayerUnlockable
{
	public Image selectableImage;
	public int cost;
	public MeshRenderer cubeMeshRenderer;
	public Image overlay;
}
