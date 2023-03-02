using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemObtainedHint : MonoBehaviour
{
    public bool isActive;
    public GameObject hintBox;
    public Transform hintBoxTransform;
    public Image itemImage;
    public Text nameHolder;
    public List<Item> items;

    private void Update() {
        if (items.Count != 0 && isActive && !DialogueManager.instance.inDialogue)
        {
            isActive = false;
            StartCoroutine(Display());
        }
    }

    public void Show(Item item) {
        isActive = true;
        items.Add(item);
        itemImage.sprite = item.itemImage;
        itemImage.preserveAspect = true;
        nameHolder.text = item.itemName;
    }

    IEnumerator Display() {
        
        hintBox.SetActive(true);
        
        for (int j = 0; j < items.Count; j++)
        {
            itemImage.sprite = items[j].itemImage;
            nameHolder.text = items[j].itemName;
        
            for (float i = 0; i < 840; i += 40f)
            {
                hintBoxTransform.position = new Vector3(hintBoxTransform.position.x - 40, 
                        hintBoxTransform.position.y, hintBoxTransform.position.z);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1f);

            for (float i = 0; i < 840; i += 40f)
            {
                hintBoxTransform.position = new Vector3(hintBoxTransform.position.x + 40, 
                        hintBoxTransform.position.y, hintBoxTransform.position.z);
                yield return new WaitForEndOfFrame();
            }
        }
        hintBox.SetActive(false);
        items.Clear();
    }
}
