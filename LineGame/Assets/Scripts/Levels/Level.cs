using System;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;

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
	public ParticleSystem[] finishingParticleSystems;
	public Rigidbody[] TreasureChestCoins;


	

	// Render our Path and Road when it becomes visible
	private void OnEnable()
	{
		pathMeshCreator.DrawPath();
		roadMeshCreator.DrawPath();
	}
}