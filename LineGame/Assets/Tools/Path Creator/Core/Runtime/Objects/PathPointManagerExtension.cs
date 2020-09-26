using System.Collections.Generic;
using UnityEngine;

namespace PathCreation
{
    [System.Serializable]
    public class PathPointManagerExtension : MonoBehaviour
    {
        // Used to descide how to order our anchor points in our path
        public enum AbsolutePoints
        {
            FirstPoint,
            SecondPoint,
            Random,
            Alternate,
        }

        public PathCreator pathCreator;
        public GameObject PathTextureObject;

        public MeshCollider pathMeshCollider; 

        public Transform[] snapPoints;

        //public List<Vector3> cachedPoints = new List<Vector3>();
        //public List<Vector3> controlPoints = new List<Vector3>();
        //public List<Vector3> secondaryPoints = new List<Vector3>();

        [Space(20)]
        public AbsolutePoints _ansolutePoint = AbsolutePoints.FirstPoint;
        public bool _alternateFactor = true;
        public float _anchorPointDistance = 0.5f;

        [Space(20)]
        public bool EnableSubscribers = false;
        public bool destroySubmeshes = false;
        public PathPointManagerExtension[] subscribers;


        private void Awake()
        {
            if (pathCreator == null)
            {
                TryGetComponent(out pathCreator);
            }

            if (pathMeshCollider == null && PathTextureObject != null)
            {
                PathTextureObject.TryGetComponent(out pathMeshCollider);
            }
        }

        //public void RandomizePoints(Vector3[] points)
        //{
        //    for (int i = 0; i < snapPoints.Length; i++)
        //    {
        //        snapPoints[i].position += points[i];
        //    }

        //    Debug.Log("Points Randomized");
        //}


        // Could be improved so that the anchor points are placed between the Control points to smooth out the path
        // and avoid points being placed on the incorrect side of the Control points
        public void UpdatePoints()
        {
            if (snapPoints.Length == 0)
            {
                Debug.LogError($"List of Snap Points of PathPointManagerExtensions @ {gameObject.name} is Empty!", gameObject);
                return;
            }

            int _snapPointCounter = 0;

            for (int i = 0; i < pathCreator.bezierPath.points.Count; i++)
            {
                //If is Control Point
                if (i % 3 == 0)
                {
                    // Move our Control Point
                    pathCreator.bezierPath.MovePoint(i, snapPoints[_snapPointCounter].position + new Vector3(0, 0, Random.Range(0.002f, 0.003f)));

                    // Move our Secondary Point(s)
                    // If this was our first control point, we only have one seconday point to move with a +1 index
                    if (i == 0)
                    {
                        pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[0].position, snapPoints[1].position, _anchorPointDistance));
                    }
                    // Else if it was our last control point, we only have one secondary point to move with a -1 index
                    else if (i == pathCreator.bezierPath.points.Count - 1)
                    {
                        pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter - 1].position, snapPoints[_snapPointCounter].position, _anchorPointDistance));
                    }
                    // Else, this is a control point in the middle, adjust both secondary points 
                    else
                    {
                        switch (_ansolutePoint)
                        {
                            case AbsolutePoints.FirstPoint:
                                pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                break;
                            case AbsolutePoints.SecondPoint:
                                pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                break;

                            case AbsolutePoints.Random:
                                bool _r = Random.Range(0, 2) != 0;

                                if (_r)
                                {
                                    pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                    pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                }
                                else
                                {
                                    pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                    pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                }
                                break;

                            case AbsolutePoints.Alternate:
                                if (_alternateFactor)
                                {
                                    pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                    pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                }
                                else
                                {
                                    pathCreator.bezierPath.MovePoint(i - 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter - 1].position, _anchorPointDistance));
                                    pathCreator.bezierPath.MovePoint(i + 1, PlaceAtPoint(snapPoints[_snapPointCounter].position, snapPoints[_snapPointCounter + 1].position, _anchorPointDistance));
                                }
                                break;
                        }

                        _alternateFactor = !_alternateFactor;
                    }

                    _snapPointCounter++;
                }
            }
        }

        // Called when this Extension Manager is linked to another path
        public void LinkPoints(Vector3[] _points)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                pathCreator.bezierPath.MovePoint(i, _points[i]);
            }
        }

        // Destroys our old Mesh, and adds a new one and reasigns reference if succesfull
        public void RecreateMeshCollider()
        {
            DestroyImmediate(pathMeshCollider);
            PathTextureObject.AddComponent<MeshCollider>();
            PathTextureObject.TryGetComponent(out pathMeshCollider);
        }


        // Return the midway point between A and B;
        public Vector3 PlaceBetween(Vector3 _positionA, Vector3 _positionB, float _atDistance)
        {
            return _atDistance * (_positionA + _positionB);
        }

        public Vector3 PlaceAtPoint(Vector3 _positionA, Vector3 _positionB, float _atDistance)
        {
            return Vector3.Lerp(_positionA, _positionB, _atDistance);
        }

                // Update our points when we adjust our mesh/path
        public void UpdatePointList()
        {
            //cachedPoints = pathCreator.bezierPath.points;

            //StopAllCoroutines();

            //AdjustPoints();

            //StartCoroutine(AdjustPoints());
        }
    }
}