using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PathFollower pathfollower;

    public bool gameplayEnabled = true;


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!gameplayEnabled)
            return;

		CamFollow.instance.OnUpdate();

		pathfollower.OnUpdate();
	}
}