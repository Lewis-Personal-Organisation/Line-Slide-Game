using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
	[SerializeField]
	public RoadMeshCreator roadMeshCreator;
	public PathCreator pathCreator => roadMeshCreator.pathCreator;
	public Vector3 colliderSize;
	public float yOffset;
	public List<ColliderZone> colliderZones;
	private Transform colliderHolder;
	public List<BoxCollider> colliders = new List<BoxCollider>();

	public bool PathColliderGenComplete => colliders[colliders.Count - 1].name == "Last Generated Collider" && colliders[0].name == "First Generated Collider";
	private static readonly string colliderHolderObjName = "Generated Collider Holder";

	[Serializable]
	public class ColliderZone
	{
		public float start;
		public float end;
		public int colliderCount;
	}

	/// <summary>
	/// Finds existing colliderHolder object. If one does not exist, create one
	/// </summary>
	/// <param name="prefabStage"></param>
	private void AssignColliderHolder(/*PrefabStage prefabStage*/)
	{
		foreach (Transform child in this.transform)
		{
			if (child.name == colliderHolderObjName)
			{
				colliderHolder = child;
				return;
			}
		}

		if (colliderHolder == null)
		{
			colliderHolder = new GameObject(colliderHolderObjName).transform;
			StageUtility.PlaceGameObjectInCurrentStage(colliderHolder.gameObject);
		}
	}

	/// <summary>
	/// Sets the Collider Holder for new colliders. Old colliders are destroyed. Generates new colliders based on a list of collider zones
	/// </summary>
	protected internal void Generate()
	{
		// Find or Create the Collider Holder
		AssignColliderHolder();

		// Destroy old Objects
		foreach (var collider in colliders)
		{
			if (collider)
			{
				DestroyImmediate(collider.gameObject);
			}
		}

		// Empty our list
		colliders.Clear();

		// The distance step along the path for each collider
		float step = 0;

		// Loops each zone, generating a new distance
		for (int i = 0; i < colliderZones.Count; i++)
		{
			step = (colliderZones[i].end - colliderZones[i].start) / colliderZones[i].colliderCount;

			for (int c = 0; c < colliderZones[i].colliderCount; c++)
			{
				Vector3 colliderPos = pathCreator.path.GetPointAtTime(colliderZones[i].start + step * c);
				Quaternion colliderRot = pathCreator.path.GetRotation(colliderZones[i].start + step * c);

				GameObject go = new GameObject("Generated Collider");
				StageUtility.PlaceGameObjectInCurrentStage(go);
				go.transform.SetParent(colliderHolder, true);
				go.transform.position = colliderPos - Vector3.down * yOffset;
				go.transform.rotation = colliderRot;

				BoxCollider boxCollider = go.AddComponent<BoxCollider>();
				boxCollider.size = colliderSize;

				colliders.Add(boxCollider);
			}
		}

		// Generate the start and end colliders
		GenerateCollider(true);
		GenerateCollider(false);
	}

	/// <summary>
	/// Generates the first or last collider along a Vertex Path. 
	/// Collider size and placement is calculated from the last collider in all ranges
	/// </summary>
	public void GenerateCollider(bool isFirst)
	{
		// Spawn the temp object in the prefab heirarchy
		Transform colliderTransform = new GameObject($"{(isFirst ? "First" : "Last")} Generated Collider").transform;
		StageUtility.PlaceGameObjectInCurrentStage(colliderTransform.gameObject);
		colliderTransform.SetParent(colliderHolder, true);

		// Find the first/last point on our path
		Vector3 pathPoint = pathCreator.path.GetPointAtTime(isFirst ? 0 : 1, EndOfPathInstruction.Stop);

		// Set the object to be between the two points
		colliderTransform.transform.position = ((pathPoint + colliders[FindColliderIndex(isFirst)].transform.position) / 2F).Replace(Utils.Axis.Y, yOffset);
		
		// Attache Box Collider with appropriate position and size
		BoxCollider collider = colliderTransform.gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(pathPoint.x - colliders[FindColliderIndex(isFirst)].transform.position.x, colliderSize.x, roadMeshCreator.roadWidth * 2F);

		// Insert the collider to the start or end of the collider list
		colliders.Insert(isFirst ? 0 : colliders.Count, collider);
	}

	/// <summary>
	/// Returns the Index of the collider which is either the first or last along the path
	/// </summary>
	/// <returns></returns>
	private int FindColliderIndex(bool isFirst)
	{
		int furthestIndex = 0;
		float cachedTime = isFirst ? float.MaxValue : float.MinValue;
		float time = 0;

		for (int i = 0; i < colliders.Count; i++)
		{
			time = pathCreator.path.GetClosestTimeOnPath(colliders[i].transform.position);
			if (isFirst ? time < cachedTime : time > cachedTime)
			{
				cachedTime = time;
				furthestIndex = i;
			}
		}

		return furthestIndex;
	}
}