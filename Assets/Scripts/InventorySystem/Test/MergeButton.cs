using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeButton : MonoBehaviour
{
    public Item[] oldNItems;
    public int[] oldNQuantity;
    public Item[] oldSItems;
    public Inventory oldInventory;

    void Awake() {
        oldInventory = new Inventory();
        for (int i = 0; i < oldNItems.Length; i++) {
            oldInventory.AddTo(true, oldNItems[i], oldNQuantity[i]);
        }

        for (int i = 0; i < oldSItems.Length; i++) {
            oldInventory.AddTo(false, oldSItems[i]);
        }
    }

    public void Merge() {
        Inventory mergedInventory = Inventory.Merge(oldInventory, Inventory.instance);
        Inventory.AssignNewInventory(mergedInventory);
    }
}
