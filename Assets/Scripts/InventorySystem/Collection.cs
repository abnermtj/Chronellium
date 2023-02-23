using System;
using System.Collections.Generic;
using UnityEngine;

public class Collection
{
    private int capacity;
    private int scannedCapacity;
    public List<Countable<Item>> items;
    private Dictionary<Item, Countable<Item>> itemsTable;
    public Action onItemChanged;
    public Action<Item> onNewItemAdded;
    public Func<Collection, Item, int, bool> assertAddRule;

    // Used to define rules for the inventory, invoked before adding an item to the collection
    // Use case: For the scanned collection, per slot the item amount can only be 1
    // protected virtual bool AssertAddRule(Item item, int quantity) { return true; }

    public Collection(Func<Collection, Item, int, bool> assertAddRule, List<Countable<Item>> items = null, int capacity = 8) {
        this.items = items ?? new List<Countable<Item>>();
        this.itemsTable = new Dictionary<Item, Countable<Item>>();
        this.capacity = capacity;
        this.assertAddRule = assertAddRule;

        foreach (Countable<Item> itemStack in this.items) {
            itemsTable.Add(itemStack.Data, itemStack);
        }
    }

    public void Add(Item item, int quantity = 1) {
        if (items.Count >= capacity) {
            Debug.Log("Inventory normal capacity exceeded.");
            return;
        }

        if (!assertAddRule.Invoke(this, item, quantity)) {
            return;
        }

        // Existing item.
        if (itemsTable.ContainsKey(item)) {
            itemsTable[item].AddToStock(quantity);
        } else {
        // New item
            Countable<Item> itemStack = new Countable<Item>(item, quantity);
            itemsTable.Add(item, itemStack);
            items.Add(itemStack);
            onNewItemAdded?.Invoke(item);
        }

        onItemChanged?.Invoke();
    }
 
    public bool Contains(Item item) {
        return itemsTable.ContainsKey(item);
    }

    public int StockOf(Item item) {
        if (itemsTable.ContainsKey(item)) {
            return itemsTable[item].Stock;
        }
        return 0;
    }

    public int Size() {
        return items.Count;
    }

    public void UseItem(Item item, int quantity = 1, bool isUsedFromInventory = true) {
        if (
            itemsTable.ContainsKey(item) 
            && (!isUsedFromInventory || (item.canUseFromInventory && isUsedFromInventory)) 
            && itemsTable[item].Stock >= quantity
        ) {
            for (int i = 0; i < quantity; i++) {
                item.Use();
            }

            bool noneLeft = item.isConsumable ? itemsTable[item].RemoveStock(quantity) : false;
            if (noneLeft) {
                items.Remove(itemsTable[item]);
                itemsTable.Remove(item);
            }

            onItemChanged?.Invoke();
            return;
        }
    }

    // NOTE: These GetCopy() are required to support snapshot function required to record an immutable state of inventory
    // at rewindable time checkpoints
    public virtual Collection GetCopy() {
        List<Countable<Item>> newItems = new List<Countable<Item>>();

        foreach (Countable<Item> itemStack in items) {
            newItems.Add(itemStack.GetCopy());
        }

        return new Collection(assertAddRule, newItems);
    }
}
