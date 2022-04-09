using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
	// Path Creators
	public PathCreation.Examples.RoadMeshCreator pathMeshCreator;
	public PathCreation.PathCreator pathCreator;

	//Road Creators
	public PathCreation.Examples.RoadMeshCreator roadMeshCreator;
	public PathCreation.PathCreator roadPathCreator;

	//Water
	public MeshRenderer WaterMesh;

	// Draw our path when we spawn
	private void Start()
	{
		pathMeshCreator.DrawPath();
		roadMeshCreator.DrawPath();
	}
}
