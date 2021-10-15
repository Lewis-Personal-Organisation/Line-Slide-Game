using UnityEngine;
using System.Collections.Generic;

public class ScrollViewManager : MonoBehaviour
{
    private enum FillTypes
    {
        Half,
        Full,
    }

    [SerializeField]
    private Transform spawnPoint = null;

    [SerializeField]
    private GameObject itemPrefab = null;

    [SerializeField]
    private RectTransform content = null;

    [SerializeField]
    private float itemSizeY;

    [SerializeField]
    private List< ScrollItem> items = null;

    float maxItemCountForFill => /*1;*/ content.sizeDelta.y / itemSizeY;

    private void Awake()
    {
        itemSizeY = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
    }


    private void Start()
    {
        FillContentWithItemSlots(FillTypes.Full);
    }

    private void FillContentWithItemSlots(FillTypes _fillType)
    {
        for (int i = 0; i < maxItemCountForFill; i++)
        {
            //Vector3 _position = new Vector3(spawnPoint.position.x, maxItemCountForFill * i, spawnPoint.position.z);

            GameObject _newItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);

            _newItem.transform.SetParent(spawnPoint, true);

            items.Add(_newItem.GetComponent<ScrollItem>());

            spawnPoint.position -= Vector3.down * i;
        }
    }
}