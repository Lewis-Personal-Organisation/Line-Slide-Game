﻿using UnityEngine;

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
    [SerializeField] private bool isControllable = false;

    [Header("Player")]
    public MeshRenderer playerMeshRenderer;
    public TrailRenderer playerTrail;
    public Color PlayerTrailColour
    {
        set { playerTrail.sharedMaterial.color = value; }
	}
	[Header("Player Colours")]
	public Material playerMaterial;
	[SerializeField] private Collider playerCollider;
    public Rigidbody[] splitCubeObjects;
    public Vector3[] cachedPositions;

    [Header("Particle System")]
    public ParticleSystem playerParticles;
    public float maxLifetime = 0.8F;
    public float minLifetime = 0.3F;

    [Header("Audio")]
    public AudioSource audioSource;

    // Player front position
    public Vector3 playerFront => transform.position + transform.forward * transform.localScale.z * 0.5F;
    public bool pathComplete => pathCreator.path.GetClosestTimeOnPath(playerFront) >= 1; 
    public float timeOnPath => pathCreator.path.GetClosestTimeOnPath(playerFront);
    private bool finishLineReached => pathCreator.path.GetClosestDistanceAlongPath(transform.position) >= LevelManager.Instance.currentLevel.finishLineDistance;


	/// <summary>
	/// When the player hits a trap, stop particles and toggle colliders. Trigger end of level state
	/// </summary>
	private void OnTriggerEnter(Collider Collider)
    {
        if (Collider.CompareTag("Trap"))
		{
            playerParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            playerMeshRenderer.enabled = false;
            FireSplitCubes();
            ToggleEnabled();
			UIManager.Instance.SwitchView(UIManager.ViewStates.LevelFailed);
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
        }
        else
		{
			if (roadCreator.playerScaleTrigger == null)
				roadCreator.playerScaleTrigger += AssignPlayerScale;

			roadCreator.playerScaleTrigger.Invoke();
		}
    }

    public void FixedUpdate()
	{
		if (finishLineReached)
		{
			LevelManager.OnLevelComplete?.Invoke();
            UIManager.Instance.EnableLevelTimer(false);

			distanceTravelled += speed * Time.deltaTime;
			transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
			transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);
		}
        else
        {
			AdjustSpeed();

			if (speed == 0)
			{
				if (playerParticles.isPlaying)
				{
					playerParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
				}
				return;
			}

			distanceTravelled += speed * Time.deltaTime;

			// Move and Rotate character using distanceTravelled variable and half size
			transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f);
			transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);

            if (!playerParticles.isPlaying)
			{
				playerParticles.Play(false);
			}
		}

		UIManager.Instance.UpdateLevelProgressUI(distanceTravelled, pathCreator.path.length);
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
        }
        else
        {
			if (roadCreator.playerScaleTrigger == null)
			{
				roadCreator.playerScaleTrigger += AssignPlayerScale;
			}

			roadCreator.playerScaleTrigger?.Invoke();
		}

        // Reset our Player so they begin at the start of the level
        ResetPath();

        ResetSplitCubes();
	}

    //Increase our speed if we are touching the screen over multiple frames. If we aren't touching screen or are touching prohibited UI
    //elements, decrease our speed. Final value is clamped.
    private void AdjustSpeed()
    {
        if (UIManager.Instance.touchingOverFrames && UIManager.Instance.isTouchingUIElement == false && isControllable)
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
        Debug.Log("TEST");
        distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    /// <summary>
    /// Resets the Player path and re-enables Movement and Collisions
    /// </summary>
    public void OnLevelReset()
    {
        ResetPath();
        enabled = true;
        playerMeshRenderer.enabled = true;
		ToggleCollisions();
		ResetSplitCubes();
	}

    /// <summary>
    /// Reset the distance travelled, Player position, UI status and Player Trail
    /// </summary>
    public void ResetPath()
    {
        speed = 0;
        distanceTravelled = .25F;
		transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction) + new Vector3(0f, transform.localScale.x / 2, 0f); transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction) * Quaternion.AngleAxis(90F, Vector3.forward);
		UIManager.Instance.UpdateLevelProgressUI(distanceTravelled, pathCreator.path.length);
        playerTrail.transform.localPosition = new Vector3(0, -0.43F, -0.5F);
        playerTrail.Clear();
	}

    /// <summary>
    /// Assigns the Player Scale to fit the path size
    /// </summary>
    public void AssignPlayerScale()
    {
        this.transform.localScale = new Vector3(roadCreator.roadWidth * 2, roadCreator.roadWidth * 2, roadCreator.roadWidth * 2);
    }

    /// <summary>
    /// Toggle player collisions
    /// </summary>
    /// <returns></returns>
    public bool ToggleCollisions()
    {
        playerCollider.enabled = !playerCollider.enabled;
        return playerCollider.enabled;
    }

    /// <summary>
    /// Toggles the player and its collider
    /// </summary>
    /// <returns></returns>
    public void ToggleEnabled()
	{
        ToggleCollisions();
		this.enabled = !this.enabled;
	}

    /// <summary>
    /// Cache the Cube positions. These positions are where the cubes reset to
    /// </summary>
	public void CacheSplitCubePositions()
	{
		cachedPositions = new Vector3[splitCubeObjects.Length];
		for (int i = 0; i < splitCubeObjects.Length; i++)
		{
			cachedPositions[i] = splitCubeObjects[i].transform.localPosition;
		}
	}

    /// <summary>
    /// Set the Cubes positions back to pre-fire state
    /// </summary>
    private void ResetSplitCubes()
    {
		splitCubeObjects[0].transform.parent.gameObject.SetActive(false);
		for (int i = 0; i < splitCubeObjects.Length; i++)
		{
			splitCubeObjects[i].transform.localPosition = cachedPositions[i];
		}
	}

    /// <summary>
    /// Fire the Split Cubes when we die
    /// </summary>
    public void FireSplitCubes()
    {
		splitCubeObjects[0].transform.parent.gameObject.SetActive(true);
		foreach (Rigidbody splitCube in splitCubeObjects)
		{
			splitCube.AddTorque(new Vector3(UnityEngine.Random.Range(1, 3), UnityEngine.Random.Range(2, 5), UnityEngine.Random.Range(1, 3)));
			splitCube.AddExplosionForce(1.6F, splitCubeObjects[0].transform.parent.position, 1);
		}
	}

    /// <summary>
    /// Adjusts the Trail following position, so the trail encapuslates the vertices positions when traversing curved paths sections
    /// </summary>
    public void SetTrailDistance()
    {
		playerTrail.transform.localPosition = new Vector3(0, -0.43F, 0 /*-0.45F*/);
	}

    /// <summary>
    /// Can the Player control the Path Follower?
    /// </summary>
    public void SetPlayerControl(bool choice)
    {
        isControllable = choice;
    }
}