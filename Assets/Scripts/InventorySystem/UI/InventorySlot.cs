using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    Countable<Item> itemStack;
    public Collection referencedCollection;
    public Image icon;
    public Text itemName;
    public Text amount;
    public Button slotButton;

    public void SetItem(Countable<Item> newItemStack) {
        itemStack = newItemStack;
       
        icon.sprite = newItemStack.Data.icon;
        icon.preserveAspect = true;
        itemName.text = newItemStack.Data.itemName;
        amount.text = newItemStack.Stock.ToString();

        icon.enabled = true;
        itemName.enabled = true;
        amount.enabled = true;
        slotButton.interactable = true;
    }

    public void ClearSlot() {
        itemStack = null;

        icon.sprite = null;
        itemName.text = "";
        amount.text = "";

        icon.enabled = false;
        itemName.enabled = false;
        amount.enabled = false;
        slotButton.interactable = false;
    }

    public void UseItem() {
        if (itemStack != null) {
            Debug.Log($"Using {itemStack.Data.itemName}");
            referencedCollection.UseItem(itemStack.Data);
        }
    }
}
