using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IMenu : MonoBehaviour
{
    public RectTransform item;

    //[HideInInspector]
    public Vector2 defaultPosition = new Vector2(0F, 0F);
    [HideInInspector]
    public Vector2 activePosition = new Vector2(0F, 0F);
}