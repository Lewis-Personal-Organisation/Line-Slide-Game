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
			//int count = pathCreator.bezierPath.points.Count - _subscriber.pathCreator.bezierPath.points.Count;
			//if (count > 0)
			//{
			//	Debug.Log($"Creator has {count} more points. Adding {count} points");

			//	for (int i = 0; i < count; i++)
			//	{
			//		_subscriber.pathCreator.bezierPath.AddSegmentToEnd(Vector3.zero);
			//	}
			//}
			//else if (count < 0)
			//{
			//	count = Mathf.Abs(count);
			//	Debug.Log($"Creator has {count} LESS points. DELETING {count} points");
			//	for (int i = 0; i < Mathf.Abs(count); i++)
			//	{
			//		_subscriber.pathCreator.bezierPath.DeleteSegment(_subscriber.pathCreator.bezierPath.points.Count - 1);
			//	}
			//}

			// Issue: If pathCreator has more points than subscriber, 
			for (int i = 0; i < pathCreator.bezierPath.points.Count; i++)
			{
				_subscriber.pathCreator.bezierPath.MovePoint(i, pathCreator.bezierPath.points[i]);
				Debug.Log($"Moved Point {i} on {_subscriber.gameObject.name}");
			}

			// Tell the Path Creator its path has been updated, which creates the road mesh
			_subscriber.pathCreator.EditorData.PathTransformed();

			UnityEditor.SceneView.RepaintAll();
		}
#endif
	}
}
