using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddItemButton : MonoBehaviour
{
    public Item item;
    public int quantity;

    void Start() {
        if (Inventory.instance == null) {
            Inventory.AssignNewInventory(new Inventory());
        }
    }

    public void PutItemInNormalInventory() {
        Inventory.instance.AddTo(true, item, quantity);
    }

    public void PutItemInScannedInventory() {
        Inventory.instance.AddTo(false, item, quantity);
    }

    void OnDestroy() {
        Inventory.instance = null;
    }
}
