using UnityEngine;

[CreateAssetMenu(menuName="Item")]
public class Item : ScriptableObject
{
    // NOTE: Each item should have a unique name
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public Sprite itemImage;
    // Some items cannot be used from inventory but are auto consumed when interacting with target (legacy from Kizuna)
    public bool canUseFromInventory = true;
    // Items like map or files are not consumable and thus cannot decrease in amount
    public bool isConsumable = true;
    public EventManager.Event itemUsedEvent;

    // The behaviour of the item when the player uses
    // the item in the inventory.
    public virtual void Use() {
        // Non consumables will be inspected instead via zoom box
        if (!isConsumable) Inventory.instance.onItemInspected?.Invoke(this);
        // InputManager.itemUseButtonActivated = true;
        EventManager.InvokeEvent(itemUsedEvent);
    }

    public override bool Equals(object other) {
        if (other is Item) {
            return ((Item)other).itemName == itemName;
        }

        return false;
    }

    public override int GetHashCode() {
        return itemName.GetHashCode();
    }
}
