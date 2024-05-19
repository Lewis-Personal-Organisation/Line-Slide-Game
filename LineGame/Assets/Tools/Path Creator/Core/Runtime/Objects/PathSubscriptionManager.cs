using System;
using System.Collections;
using UnityEngine;

/// <summary>
///  An extension to the Path Creator script
///  Allows other paths to be subscribed to this path for synchronising path points
/// </summary>

public class PathSubscriptionManager : MonoBehaviour
{
	public PathCreator pathCreator;
	public RoadMeshCreator[] roadMeshSubscribers;

	public void UpdateSubscribers()
	{
#if UNITY_EDITOR
		foreach (RoadMeshCreator _subscriber in roadMeshSubscribers)
		{
			// Issue: If pathCreator has more points than subscriber, Dev needs to assign equal number of points
			_subscriber.pathCreator.bezierPath = pathCreator.bezierPath;

			// Tell the Path Creator its path has been updated, which creates the road mesh
			_subscriber.pathCreator.EditorData.PathTransformed();

			UnityEditor.SceneView.RepaintAll();
		}
#endif
	}
}
