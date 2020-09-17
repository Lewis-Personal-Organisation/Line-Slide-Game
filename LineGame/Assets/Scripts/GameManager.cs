using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private enum Scenes
    {
        Movement,
        UI,
    }

    public static GameManager instance;

    Scenes currentScene = Scenes.Movement;

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