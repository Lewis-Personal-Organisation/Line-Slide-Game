using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
	[SerializeField]
	public PathCreator pathCreator;
	public Vector3 colliderSize;
	public float yOffset;
	public List<ColliderDistanceRange> distanceRanges;
	private Transform colliderHolder;
	public List<BoxCollider> colliders = new List<BoxCollider>();

	[Serializable]
	public class ColliderDistanceRange
	{
		public float start;
		public float end;
		public float colliderCount;
		[HideInInspector] public float step;
	}

	internal void Generate()
	{
		if (colliderHolder == null)
		{
			colliderHolder = new GameObject("Generated Collider Holder").transform;
			StageUtility.PlaceGameObjectInCurrentStage(colliderHolder.gameObject);
		}

		// Destroy old Objcts
		foreach (var collider in colliders)
		{
			if (collider)
				DestroyImmediate(collider.gameObject);
		}

		colliders.Clear();

		float step = 0;

		for (int i = 0; i < distanceRanges.Count; i++)
		{
			step = (distanceRanges[i].end - distanceRanges[i].start) / distanceRanges[i].colliderCount;

			for (int c = 0; c < distanceRanges[i].colliderCount; c++)
			{
				Vector3 colliderPos = pathCreator.path.GetPointAtTime(distanceRanges[i].start + step * c);
				Quaternion colliderRot = pathCreator.path.GetRotation(distanceRanges[i].start + step * c);
				
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
	}
}