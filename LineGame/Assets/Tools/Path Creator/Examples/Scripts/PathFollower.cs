using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

// Moves along a path at constant speed.
// Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
public class PathFollower : MonoBehaviour
{
    public PathCreator pathCreator => LevelManager.Instance.currentLevel.roadPathCreator;
    public RoadMeshCreator roadCreator => LevelManager.Instance.currentLevel.roadMeshCreator;

    public EndOfPathInstruction endOfPathInstruction;

    [Header("Path Options")]
    public float speed = 5;
    public float maxSpeed = 10;
    public float accelerationMultiplier = 5;
    public float distanceTravelled = 0;
	public bool scaleToPathWidth;

    [Header("Player")]
    public TrailRenderer playerTrail;
    public Color PlayerTrailColour
    {
        set { playerTrail.sharedMaterial.color = value; }
    }
    [SerializeField] private Collider trapCollider;

    [Header("Particle System")]
    public ParticleSystem pigParticles;
    ParticleSystem.MainModule pigParticlesModule;
    public float maxLifetime = 0.8F;
    public float minLifetime = 0.3F;

	//Path Queries
	public int pathPercentComplete => (int)Math.Round(Mathf.Clamp(distanceTravelled / pathCreator.path.length, 0F, 1F) * 100F, MidpointRounding.AwayFromZero);
	public bool pathComplete => Mathf.Clamp(distanceTravelled / pathCreator.path.length, 0, 1) == 1;
	private bool finishLineReached => pathCreator.path.GetClosestDistanceAlongPath(transform.position) >= LevelManager.Instance.currentLevel.finishLineDistance;
	//public float finishToEndPercent => Mathf.InverseLerp(LevelManager.Instance.currentLevel.finishLineDistance, LevelManager.Instance.currentLevel.treasureChestDistance, pathCreator.path.GetClosestDistanceAlongPath(transform.position)) * 100;
	public bool PSOneReached => pathCreator.path.GetClosestDistanceAlongPath(transform.position) >= LevelManager.Instance.particleSystemOneDistance;
	public bool PSTwoReached => pathCreator.path.GetClosestDistanceAlongPath(transform.position) >= LevelManager.Instance.particleSystemTwoDistance;
	public bool PSThreeReached => pathCreator.path.GetClosestDistanceAlongPath(transform.position) >= LevelManager.Instance.particleSystemThreeDistance;


	private void Awake()
    {
        pigParticlesModule = pigParticles.main;
        pigParticlesModule.startLifetime = Mathf.Clamp(maxLifetime - speed, minLifetime, maxLifetime);
    }

	/// <summary>
	/// When the player hits a trap and UI is in the Gameplay state, trigger end of level state
	/// </summary>
	/// <param name="Collider"></param>
	private void OnTriggerEnter(Collider Collider)
    {
        if (Collider.CompareTag("Trap"))
        {
            UITouch.Instance.SwitchView(UITouch.ViewStates.LevelFailed);
            ToggleEnabled();
		}
    }

    // When our inspector changes, we want to make sure the Player is scaled according to our Road Mesh
    private void OnValidate()
    {
        if (LevelManager.Instance == null)
            return;

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

    public void LateUpdate()
	{
		if (finishLineReached)
		{
			LevelManager.OnLevelComplete?.Invoke();
			distanceTravelled += speed * Time.deltaTime;
			transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
			transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);
			LevelManager.Instance.UpdateUIProgress(distanceTravelled, pathCreator.path.length);
		}
        else
        {
			AdjustSpeed();

			if (speed == 0)
			{
				if (pigParticles.isPlaying)
				{
					pigParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				}
				return;
			}

			distanceTravelled += speed * Time.deltaTime;

			// Move and Rotate character using distanceTravelled variable and half size
			transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
			transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);

			LevelManager.Instance.UpdateUIProgress(distanceTravelled, pathCreator.path.length);

            pigParticlesModule.startLifetime = Mathf.Clamp(maxLifetime - speed, minLifetime, maxLifetime);

            if (!pigParticles.isPlaying)
			{
				pigParticles.Play(false);
			}
		}
    }

    /// <summary>
    /// Setup our Path Follower variables, now that our level is loaded
    /// </summary>
    /// <param name="path"></param>
    /// <param name="road"></param>
    public void Setup()
    {
        pathCreator.pathUpdated += OnPathChanged; // Subscribed to pathUpdated event so that we're notified if the path changes during the game

        // Make sure the Player is scaled according to our Road Mesh
        if (!scaleToPathWidth)
        {
            roadCreator.playerScaleTrigger -= AssignPlayerScale;
            return;
        }

        if (roadCreator.playerScaleTrigger == null)
        {
            roadCreator.playerScaleTrigger += AssignPlayerScale;
        }

        roadCreator.playerScaleTrigger.Invoke();

        // Reset our Player so they begin at the start of the level
        ResetPath();
    }

    //Increase our speed if we are touching the screen over multiple frames. If we aren't touching screen or are touching prohibited UI
    //elements, decrease our speed. Final value is clamped.
    private void AdjustSpeed()
    {
        if (UITouch.Instance.touchingOverFrames && UITouch.Instance.isTouchingUIElement == false)
        {
            speed += Time.deltaTime * accelerationMultiplier;
        }
        else
        {
            speed -= Time.deltaTime * accelerationMultiplier;
        }

        speed = Mathf.Clamp(speed, 0, maxSpeed);
    }

    public void ApplyCruiseSpeed()
    {
        speed = 3F;
    }

    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged()
    {
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    /// <summary>
    /// Resets the Player path and re-enables Movement and Collisions
    /// </summary>
    public void OnLevelReset()
    {
        ResetPath();
        enabled = true;
        ToggleCollisions();
    }

    /// <summary>
    /// Reset the distance travelled, Player position, UI status and Player Trail
    /// </summary>
    public void ResetPath()
    {
        speed = 0;
        distanceTravelled = 0;
		transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f); transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);
        LevelManager.Instance.UpdateUIProgress(distanceTravelled, pathCreator.path.length);
        playerTrail.Clear();
	}

    public void AssignPlayerScale()
    {
        this.transform.localScale = new Vector3(roadCreator.roadWidth * 2, roadCreator.roadWidth * 2, roadCreator.roadWidth * 2);
    }

    public bool ToggleCollisions()
    {
        trapCollider.enabled = !trapCollider.enabled;
        return trapCollider.enabled;
    }

    public bool ToggleEnabled()
	{
		trapCollider.enabled = !trapCollider.enabled;
		this.enabled = !this.enabled;
        return this.enabled;
	}
}