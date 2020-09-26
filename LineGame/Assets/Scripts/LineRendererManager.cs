using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererManager : MonoBehaviour
{
    public static LineRendererManager instance;

    public PathFollower pathFollower;

    public LineRenderer lineRenderer;

    public bool isRunning = false;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        lineRenderer.SetPosition(0, pathFollower.pathCreator.path.GetPointAtDistance(0, PathCreation.EndOfPathInstruction.Stop));
    }

    public void InsertPoint()
    {
        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, pathFollower.pathCreator.path.GetPointAtDistance(pathFollower.distanceTravelled, PathCreation.EndOfPathInstruction.Stop));
    }

    public void SetLineRendererToPlayer()
    {
        lineRenderer.SetPosition(lineRenderer.positionCount-1, pathFollower.pathCreator.path.GetPointAtDistance(pathFollower.distanceTravelled, PathCreation.EndOfPathInstruction.Stop));
    }
}