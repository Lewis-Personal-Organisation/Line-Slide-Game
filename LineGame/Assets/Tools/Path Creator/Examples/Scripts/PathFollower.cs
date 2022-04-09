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

        [Header("Path Options")]
        public float speed = 5;
        public float maxSpeed = 10;
        public float accelerationMultiplier = 5;
        public float distanceTravelled = 0;
        public bool doFollow = true;
        public bool scaleToPathWidth;
        public bool correctFlatPathRotation = false;

        [Header("Trail Renderer")]
        public TrailRenderer playerTrail;

        [Header("Particle System")]
        public ParticleSystem pigParticles;
        ParticleSystem.MainModule pigParticlesModule;
        public float maxLifetime = 0.8F;
        public float minLifetime = 0.3F;

        [Header("Level Progress")]
        [SerializeField] private LevelManager levelProgress;


        private void OnTriggerEnter(Collider trap)
        {
            if (trap.CompareTag("Trap"))
            {
                ResetPath();
            }
        }

        // When our inspector changes, we want to make sure the Player is scaled according to our Road Mesh
        private void OnValidate()
        {
            if (pathCreator == null)
                return;

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
            pigParticlesModule = pigParticles.main;
            pigParticlesModule.startLifetime = Mathf.Clamp(maxLifetime - speed, minLifetime, maxLifetime);

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

            ResetPath();
        }

        public void OnUpdate()
        {
            if (!doFollow)
                return;

            ScaleSpeed();

            if (speed * Time.deltaTime == 0)
            {
                if (pigParticles.isPlaying)
                    pigParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                return;
            }

            // Needs moving
            if (UITouch.instance.tapToPlay.gameObject.activeSelf)
            {
				Vector2 startPos = CanvasUtils.GetPos(levelProgress.image.rectTransform, Canvas.Top, UITouch.instance.canvas.scaleFactor, UITouch.instance.canvasTransform);
                levelProgress.image.rectTransform.Move(this, startPos, levelProgress.onscreenPos, 1.2F, CurveType.Exponential);
            }

            distanceTravelled += speed * Time.deltaTime;

            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
            transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * ((correctFlatPathRotation == true) ? Quaternion.AngleAxis(90F, Vector3.forward) : Quaternion.identity);

            // Tells use where between 0 and 1 we are on the path
            //Debug.Log(pathCreator.path.GetClosestTimeOnPath(transform.position));

            levelProgress.UpdateUI(distanceTravelled, pathCreator.path.length);

            pigParticlesModule.startLifetime = Mathf.Clamp(maxLifetime - speed, minLifetime, maxLifetime);

            if (!pigParticles.isPlaying)
            {
                pigParticles.Play(false);
            }
        }

        //Increase our speed if we are touching the screen over multiple frames. If we aren't touching screen or are touching prohibited UI
        //elements, decrease our speed. Final value is clamped.
        private void ScaleSpeed()
        {
            if (UITouch.instance.touchingOverFrames && UITouch.isTouchingUIItem == false)
            {
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
            Debug.Log($"Path changed");
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        public async void ResetPath()
        {
            while(pathCreator == null)
			{
                await System.Threading.Tasks.Task.Yield();
			}

            distanceTravelled = 0;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f); transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * ((correctFlatPathRotation == true) ? Quaternion.AngleAxis(90F, Vector3.forward) : Quaternion.identity);
            levelProgress.UpdateUI(distanceTravelled, pathCreator.path.length);

            playerTrail.Clear();
        }

        public void AssignPlayerScale()
        {
            this.transform.localScale = new Vector3(roadCreator.roadWidth * 2, roadCreator.roadWidth * 2, roadCreator.roadWidth * 2);
        }
    }
}