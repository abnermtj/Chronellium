using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    int index;
    [SerializeField]
    GameObject[] items;
    GameObject currentItem;
    [SerializeField]
    GameObject flicker;
    [SerializeField]
    GameObject text;
    bool hasSelected;
    // Start is called before the first frame update
    void Start()
    {
        currentItem = items[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (hasSelected) {
            return;
        }

        if (Input.GetButtonDown("Submit")) {
            hasSelected = true;
            StartCoroutine(Selected());
        }

        if (!(Input.GetKeyDown("left") || Input.GetKeyDown(KeyCode.A))
            && !(Input.GetKeyDown("right") || Input.GetKeyDown(KeyCode.D))) {
            return;
        }

        if (Input.GetKeyDown("left") || Input.GetKeyDown(KeyCode.A)) 
            index--;
        if (Input.GetKeyDown("right") || Input.GetKeyDown(KeyCode.D))
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

    IEnumerator Selected() {
        text.SetActive(true);
        for (int i = 0; i <= 4; i++) {
            flicker.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            flicker.SetActive(false);
            yield return new WaitForSeconds(0.02f);
        }
        text.SetActive(false);
        yield return null;
        hasSelected = false;
    } 
}
