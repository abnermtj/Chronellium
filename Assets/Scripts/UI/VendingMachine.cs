using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    int index;
    [SerializeField]
    GameObject[] items;
    GameObject currentItem;
    // Start is called before the first frame update
    void Start()
    {
        currentItem = items[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown("left") && !Input.GetKeyDown("right")) {
            return;
        }
        
        if (Input.GetKeyDown("left")) 
            index--;
        if (Input.GetKeyDown("right"))
            index++;
        
        if (index >= items.Length) {
            index = 0;
        } else if (index < 0) {
            index = items.Length - 1;
        }

        currentItem.SetActive(false);
        currentItem = items[index];
        currentItem.SetActive(true);
    }
}
