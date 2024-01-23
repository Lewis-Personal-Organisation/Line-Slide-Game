using System;
using UnityEngine;

[ExecuteInEditMode]
public class Level : MonoBehaviour
{
	public enum LevelDifficulty
	{
		Beginner,
		Intermediate,
		Hard,
		Impossible
	}

	[Header("Difficulty")]
	[SerializeField] LevelDifficulty difficulty;
	public LevelDifficulty Difficulty { get { return difficulty; } }

	// Path Creators
	[Header("Path Editors")]
	public RoadMeshCreator pathMeshCreator;
	public PathCreator pathCreator;
	public MeshRenderer pathRenderer;

	//Road Creators
	public RoadMeshCreator roadMeshCreator;
	public PathCreator roadPathCreator;
	public MeshRenderer roadRenderer;

	//Water
	[Header("Water")]
	public MeshRenderer WaterMesh;

	[Header("Finish Line")]
	public Transform finishLineTransform;
	public float finishLineDistance => roadPathCreator.path.GetClosestDistanceAlongPath(finishLineTransform.position);
	public float treasureChestDistance => roadPathCreator.path.GetClosestDistanceAlongPath(treasureChestPivot.position);
	[Header("End of Level Objects")]
	public Transform treasureChestPivot;
	public ParticleSystem[] particles;


	private void Awake()
	{
#if UNITY_EDITOR
		if (UnityEditor.EditorApplication.isPlaying == false)
			return;
#endif
		switch (difficulty)
		{
			case LevelDifficulty.Beginner:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = LevelManager.Instance.beginnerTrailColour;
				LevelManager.Instance.progressImage.color = LevelManager.Instance.beginnerTrailColour;
				this.WaterMesh.material.SetColor("_WaterColor", LevelManager.Instance.beginnerWaterColour);
				break;
			case LevelDifficulty.Intermediate:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = LevelManager.Instance.IntermediateTrailColour;
				GameManager.Instance.playerPathFollower.playerTrail.sharedMaterial.color = LevelManager.Instance.IntermediateTrailColour;
				this.WaterMesh.material.SetColor("_WaterColor", LevelManager.Instance.IntermediateWaterColour);
				break;
			case LevelDifficulty.Hard:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = LevelManager.Instance.hardTrailColour;
				LevelManager.Instance.progressImage.color = LevelManager.Instance.hardTrailColour;
				this.WaterMesh.material.SetColor("_WaterColor", LevelManager.Instance.hardWaterColour);
				break;
			case LevelDifficulty.Impossible:
				GameManager.Instance.playerPathFollower.PlayerTrailColour = LevelManager.Instance.impossibleTrailColour;
				LevelManager.Instance.progressImage.color = LevelManager.Instance.impossibleTrailColour;
				this.WaterMesh.material.SetColor("_WaterColor", LevelManager.Instance.impossibleWaterColour);
				break;
		}
	}

	// Render our Path and Road when it becomes visible
	private void OnEnable()
	{
		pathMeshCreator.DrawPath();
		roadMeshCreator.DrawPath();
	}
}