using PathCreation;
using PathCreation.Examples;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TextBounce textBounce;

    public PathFollower pathfollower;

    public bool gameplayEnabled = true;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //MenuManager.instance.MoveMenu(Menu.Test, Curves.CenterToRight);
        //Fader.instance.Fade(Menu.Test, 1);
        //Blurer.instance.Blur(Menu.Test, 10, 2);
    }

    private void Update()
    {
        if (!gameplayEnabled)
            return;

        CamFollow.instance.OnUpdate();

        pathfollower.OnUpdate();
    }
}