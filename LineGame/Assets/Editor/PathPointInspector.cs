using UnityEngine;
using UnityEditor;

namespace PathCreation
{
    [CustomEditor(typeof(PathCreation.PathPointManagerExtension))]
    public class PathPointInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            PathCreation.PathPointManagerExtension instance = (PathCreation.PathPointManagerExtension)target;

            if (GUILayout.Button("Update"))
            {
                instance.UpdatePoints();
                instance.RecreateMeshCollider();
            }

            //This draws the default screen.  You don't need this if you want
            //to start from scratch, but I use this when I'm just adding a button or
            //some small addition and don't feel like recreating the whole inspector.
            DrawDefaultInspector();

            //if (GUILayout.Button("RandomizePoints"))
            //{
            //    instance.RandomizePoints(UITouch.instance.randomPositions);
            //}
            

            // If we have subscribing paths, and we link more than one path, enable the Update Subscribers button
            // Then, if we click the button, for each subscriber, update their points with the Main path points. 
            // Also rebuild or destroy their mesh, depending on if we need the duplicate mesh
            if (instance.EnableSubscribers && instance.subscribers.Length > 0)
            {
                if (GUILayout.Button("Update Subscribers"))
                {
                    foreach (PathPointManagerExtension _subscriber in instance.subscribers)
                    {
                        _subscriber.LinkPoints(instance.pathCreator.bezierPath.points.ToArray());

                        if (instance.destroySubmeshes)
                        {
                            DestroyImmediate(_subscriber.pathMeshCollider);
                        }
                        else
                        {
                            _subscriber.RecreateMeshCollider();
                        }
                    }
                }
            }
        }
    }
}