using System;
using UnityEngine;

public class Inventory {
    public static Inventory instance;
    public Action<Item> onItemInspected;
    public Collection NormalCollection { get; private set; }
    public Collection ScannedCollection { get; private set; }
    private int defaultScannedCollectionCapacity = 4;
    private Func<Collection, Item, int, bool> normalAddRule = (collection, item, quantity) => true;
    // Each slot in scanned collection can only contain one item of stock count 1
    private Func<Collection, Item, int, bool> scanAddRule = (collection, item, quantity) => !collection.Contains(item) && quantity == 1;

    // Used to determine inventory state after travelling to the past, normal collection reverts, scanned collection persists
    public static Inventory Merge(Inventory pastInventory, Inventory preLeapInventory) {
        return new Inventory(pastInventory.NormalCollection.GetCopy(), preLeapInventory.ScannedCollection.GetCopy());
    }

    public static void AssignNewInventory(Inventory newInventory) {
        instance = newInventory;
        EventManager.InvokeEvent(EventManager.Event.INVENTORY_CHANGED);
    }

    public Inventory(Collection normalCollection = null, Collection scannedCollection = null) {
        NormalCollection = normalCollection ?? new Collection(normalAddRule);
        ScannedCollection = scannedCollection ?? new Collection(scanAddRule, null, defaultScannedCollectionCapacity);
    }

    public void AddTo(bool isNormal, Item item, int quantity = 1) {
        Debug.Log($"{item.itemName} added to Inventory");
        if (isNormal) {
            NormalCollection.Add(item, quantity);
        } else {
            ScannedCollection.Add(item, quantity);
        } 
    }
 
    public bool Contains(Item item) {
        return NormalCollection.Contains(item) || ScannedCollection.Contains(item);
    }

    public int AmountOf(Item item) {
        return NormalCollection.StockOf(item) + ScannedCollection.StockOf(item);
    }
}
