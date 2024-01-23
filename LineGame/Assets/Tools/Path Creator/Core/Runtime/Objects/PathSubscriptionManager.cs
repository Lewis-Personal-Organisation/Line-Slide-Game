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

		Debug.Log($"Moving points on {pathCreator.gameObject.name}", pathCreator.gameObject);

		foreach (RoadMeshCreator _subscriber in roadMeshSubscribers)
		{
			for (int i = 0; i < pathCreator.bezierPath.points.Count; i++)
			{
				_subscriber.pathCreator.bezierPath.MovePoint(i, pathCreator.bezierPath.points[i]);
				Debug.Log($"Moved Point {i} on {_subscriber.gameObject.name}");
			}

			// Tell the Path Creator its path has been updated, which creates the road mesh
			_subscriber.pathCreator.EditorData.PathTransformed();

			UnityEditor.SceneView.RepaintAll();

			// Does the same as above - not needed
			//_subscriber.DrawPath();
		}
#endif
	}
}
