using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public RoadMeshCreator roadCreator;
        public EndOfPathInstruction endOfPathInstruction;

        public float speed = 5;
        public float maxSpeed = 10;
        public float accelerationMultiplier = 5;
        public float distanceTravelled = 0;

        public bool doFollow = true;
        public bool scaleToPathWidth;

        public Vector3 cachedPosition = Vector3.zero;

        public int travelCounter = 1;
        public int travelIncrement = 2;
        public bool addPoint = false;


        // When our inspector changes, we want to make sure the Player is scaled according to our Road Mesh
        private void OnValidate()
        {
            if (!scaleToPathWidth)
            {
                roadCreator.playerScaleTrigger -= AssignPlayerScale;
                return;
            }

            if (roadCreator.playerScaleTrigger == null)
                roadCreator.playerScaleTrigger += AssignPlayerScale;

            roadCreator.playerScaleTrigger.Invoke();
        }

        void Start() 
        {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;

                // When we start, make sure the Player is scaled according to our Road Mesh
                if (!scaleToPathWidth)
                {
                    roadCreator.playerScaleTrigger -= AssignPlayerScale;
                    return;
                }

                if (roadCreator.playerScaleTrigger == null)
                    roadCreator.playerScaleTrigger += AssignPlayerScale;

                roadCreator.playerScaleTrigger.Invoke();
            }

            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

            LineRendererManager.instance.SetLineRendererToPlayer();
        }

        public void OnUpdate()
        {
            if (!doFollow)
                return;

            ScaleSpeed();

            if (pathCreator != null)
            {
                if (speed * Time.deltaTime != 0)
                {
                    distanceTravelled += speed * Time.deltaTime;
                    transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
                    transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                    //QueryPlayerLocation();

                    cachedPosition = transform.position;

                    MenuManager.instance.UpdateLevelProgress(distanceTravelled, pathCreator.path.length);

                    if (distanceTravelled > travelCounter)
                    {
                        travelCounter += travelIncrement;
                        addPoint = true;
                    }

                    LineRendererManager.instance.SetLineRendererToPlayer();
                }
            }
        }

        private void LateUpdate()
        {
            if (addPoint)
            {
                LineRendererManager.instance.InsertPoint();
                addPoint = false;
            }
        }

        private void QueryPlayerLocation()
        {
            //MeshPathColourManager.instance.isRunning = cachedPosition != transform.position;
            if (cachedPosition != transform.position && LineRendererManager.instance.isRunning == false)
            {
                //MeshPathColourManager.instance.StartCoroutine(MeshPathColourManager.instance.ISetColour());
            }
        }

        private void ScaleSpeed()
        {
            //Breaks VS like a G
            //speed = Mathf.Clamp(UITouch.instance.touchingOverFrames ? (speed += (Time.deltaTime * 5)) : (speed -= (Time.deltaTime * 5)), 0, 5));

            if (UITouch.instance.touchingOverFrames)
            {
                if (UITouch.instance.hitResults.Count != 0)
                    return;

                speed += (Time.deltaTime * accelerationMultiplier);
            }
            else
            {
                speed -= (Time.deltaTime * accelerationMultiplier);
            }

            speed = Mathf.Clamp(speed, 0, maxSpeed);
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged() 
        {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        public void ResetPath()
        {
            distanceTravelled = 0;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            MenuManager.instance.UpdateLevelProgress(distanceTravelled, pathCreator.path.length);

            travelCounter = 2;
            travelIncrement = 2;
            addPoint = false;
            LineRendererManager.instance.lineRenderer.positionCount = 2;
            LineRendererManager.instance.lineRenderer.SetPosition(0, pathCreator.path.GetPointAtDistance(0, PathCreation.EndOfPathInstruction.Stop));
            LineRendererManager.instance.lineRenderer.SetPosition(1, pathCreator.path.GetPointAtDistance(0, PathCreation.EndOfPathInstruction.Stop));
        }

        public void AssignPlayerScale()
        {
            this.transform.localScale = new Vector3(roadCreator.roadWidth * 2, roadCreator.roadWidth * 2, roadCreator.roadWidth * 2);
        }
    }
}