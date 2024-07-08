using System;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
	[SerializeField]
	public RoadMeshCreator roadMeshCreator;
	public PathCreator pathCreator => roadMeshCreator.pathCreator;
	public Vector3 colliderSize;
	public Vector3 positionOffset;
	public List<ColliderZone> colliderZones;
	private Transform colliderHolder;
	public List<BoxCollider> generatedColliders = new List<BoxCollider>();

	private static readonly string colliderHolderObjName = "Generated Collider Holder";

	[Serializable]
	public class ColliderZone
	{
		public float start;
		public float end;
		public int colliderCount;
	}

#if UNITY_EDITOR
	/// <summary>
	/// Finds existing colliderHolder object. If one does not exist, create one
	/// </summary>
	/// <param name="prefabStage"></param>
	private void AssignColliderHolder()
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
		foreach (var collider in generatedColliders)
		{
			if (collider)
			{
				DestroyImmediate(collider.gameObject);
			}
		}

		// Empty our list
		generatedColliders.Clear();

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
				go.transform.position = colliderPos + positionOffset;
				go.transform.rotation = colliderRot;

				BoxCollider boxCollider = go.AddComponent<BoxCollider>();

				// If first index, move forward half of our size, then half our size
				if (c == 0)
				{
					go.transform.position += go.transform.forward * colliderSize.z * .25F;
					boxCollider.size = colliderSize.Replace(Utils.Axis.Z, colliderSize.z / 2F);
				}
				else if (c == colliderZones[i].colliderCount-1) 
				{
					//go.transform.position += go.transform.forward * colliderSize.z * .25F;
					boxCollider.size = colliderSize.Replace(Utils.Axis.Z, colliderSize.z / 2F);
				}
				else
				{
					boxCollider.size = colliderSize;
				}

				generatedColliders.Add(boxCollider);
			}
		}

		// Determind if the Start and End colliders should be generated
		if (ShouldGenerateCollider(true))
			GenerateCollider(true);

		if (ShouldGenerateCollider(false))
			GenerateCollider(false);

		generatedColliders[0].gameObject.name = "First Generated Collider";
		generatedColliders[generatedColliders.Count-1].gameObject.name = "Last Generated Collider";
	}

	/// <summary>
	/// Generates the first or last collider along a Vertex Path. 
	/// Collider size and placement is calculated from the last collider in all ranges
	/// </summary>
	public void GenerateCollider(bool isFirst)
	{
		// Spawn the temp object in the prefab heirarchy
		Transform colliderTransform = new GameObject().transform;
		StageUtility.PlaceGameObjectInCurrentStage(colliderTransform.gameObject);
		colliderTransform.SetParent(colliderHolder, true);

		// If this is the first Generated collider, put it above all other children
		if (isFirst)
			colliderTransform.SetAsFirstSibling();

		// Find the first/last point on our path
		Vector3 pathPoint = pathCreator.path.GetPointAtTime(isFirst ? 0 : 1, EndOfPathInstruction.Stop);

		// Set the object to be between the two points
		colliderTransform.transform.position = ((pathPoint + generatedColliders[FindColliderIndex(isFirst)].transform.position) / 2F).Replace(Utils.Axis.Y, positionOffset.y);

		// Attache Box Collider with appropriate position and size
		BoxCollider collider = colliderTransform.gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(pathPoint.x - generatedColliders[FindColliderIndex(isFirst)].transform.position.x, colliderSize.x, roadMeshCreator.roadWidth * 2F);

		// Insert the collider to the start or end of the collider list
		generatedColliders.Insert(isFirst ? 0 : generatedColliders.Count, collider);
	}
#endif
	/// <summary>
	/// Returns the Index of the collider which is either the first or last along the path
	/// </summary>
	/// <returns></returns>
	private int FindColliderIndex(bool isFirstCollider)
	{
		int furthestIndex = 0;
		float cachedTime = isFirstCollider ? float.MaxValue : float.MinValue;
		float time = 0;

		for (int i = 0; i < generatedColliders.Count; i++)
		{
			time = pathCreator.path.GetClosestTimeOnPath(generatedColliders[i].transform.position);
			if (isFirstCollider ? time < cachedTime : time > cachedTime)
			{
				cachedTime = time;
				furthestIndex = i;
			}
		}

		return furthestIndex;
	}

	/// <summary>
	/// Should we generate a start or end collider. If our zone includes Start or End points, don't
	/// </summary>
	/// <returns></returns>
	public bool ShouldGenerateCollider(bool isStartCollider)
	{
		foreach (ColliderZone zone in colliderZones)
		{
			if (isStartCollider ? zone.start == 0 : zone.end == 1)
			{
				return false;
			}
		}

		return true;
	}
}