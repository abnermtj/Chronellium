using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomInBox : MonoBehaviour
{
    public GameObject zoomInBox;
    public Image itemImage;
    public Text itemName;
    public Text description;

    // FIXME: Exit zoom view. Does zooming on the object result in usage?
    public void Show(Item item) {
        Debug.Log($"Zooming in to show {item.itemName} in ZoomBox");
        itemImage.sprite = item.itemImage;
        itemImage.preserveAspect = true;
        description.text = item.description;
        itemName.text = item.itemName;

        zoomInBox.SetActive(true);
    }

    public void Hide() {
        itemImage.sprite = null;
        description.text = "";
        itemName.text = "";

        zoomInBox.SetActive(false);
    }
}
