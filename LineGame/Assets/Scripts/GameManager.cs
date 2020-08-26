using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        MenuManager.instance.MoveMenu(Menu.Test, Curves.CenterToRight);

        //Fader.instance.Fade(Menu.Test, 1);

        //Blurer.instance.Blur(Menu.Test, 10, 2);
    }
}
