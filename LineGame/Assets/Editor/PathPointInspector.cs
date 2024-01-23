using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathSubscriptionManager))]
public class PathPointInspector : Editor
{
    PathSubscriptionManager instance;

    public override void OnInspectorGUI()
    {
        instance = (PathSubscriptionManager)target;

        //if (GUILayout.Button("Regenerate Positions"))
        //{
        //instance.UpdatePoints();
        //instance.RecreateMeshCollider();
        //instance.UpdateSubscribers();             
        //}

        // If we have subscribing paths, and we link more than one path, enable the Update Subscribers button
        // Then, if we click the button, for each subscriber, update their points with the Main path points. 
        // Also rebuild or destroy their mesh, depending on if we need the duplicate mesh
        if (/*instance.EnableSubscribers &&*/ instance.roadMeshSubscribers.Length > 0)
        {
            if (GUILayout.Button("Sync Subscribers"))
            {
				//instance.RecreateMeshCollider();
				instance.UpdateSubscribers();
            }
        }

        //This draws the default screen.  You don't need this if you want
        //to start from scratch, but I use this when I'm just adding a button or
        //some small addition and don't feel like recreating the whole inspector.
        DrawDefaultInspector();
    }
}
