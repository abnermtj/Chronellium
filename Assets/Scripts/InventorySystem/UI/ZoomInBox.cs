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
    public void Show(Item item)
    {
        itemImage.sprite = item.itemImage;
        itemImage.preserveAspect = true;
        description.text = item.description;
        itemName.text = item.itemName;

        if (zoomInBox.activeInHierarchy) {
            zoomInBox.SetActive(false);
        } else {
            zoomInBox.SetActive(true);
        }
    }
}
