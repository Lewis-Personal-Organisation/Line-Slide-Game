using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IMenu : MonoBehaviour
{
    public RectTransform item;

    public Menu menuItem;

    public Vector2 defaultPosition = Vector3.zero;


	private void Awake()
	{
		StartCoroutine(Setup());
	}

    private IEnumerator Setup()
	{
		yield return new WaitUntil(() => MenuManager.instance);
		MenuManager.instance.menus.Add(menuItem, this);
	}
}