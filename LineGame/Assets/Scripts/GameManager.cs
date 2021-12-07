using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PathFollower pathfollower;

    public bool gameplayEnabled = true;

    public static int levelCount => 1;


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!gameplayEnabled)
            return;

        pathfollower.OnUpdate();
	}

	private void LateUpdate()
    {
        CamFollow.instance.OnUpdate();
    }
}