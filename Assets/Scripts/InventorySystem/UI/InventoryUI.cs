using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

// Must be placed under a singleton parent.
public class InventoryUI : MonoBehaviour
{
    public ZoomInBox zoomBox;
    public ItemObtainedHint itemHint;
    public Transform itemsParent;
    public GameObject panel;
    public bool isNormal;
    private Collection referencedCollection;
    InventorySlot[] slots;
    public GameObject firstSlot;
    private Stack<Action> pageRecyclers;

    void Awake() {
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();
        pageRecyclers = new Stack<Action>();
    }

    public void Init(object input = null) {
        foreach (InventorySlot slot in slots) {
            slot.ClearSlot();
        }

        referencedCollection = isNormal ? Inventory.instance.NormalCollection : Inventory.instance.ScannedCollection;
        referencedCollection.onNewItemAdded += ShowItemHint;

        for (int i = 0; i < referencedCollection.Size(); i++) {
            slots[i].SetItem(referencedCollection.items[i]);
        }

        foreach (InventorySlot slot in slots) {
            slot.referencedCollection = referencedCollection;
        }

        Hide();
    }

    // Start is called before the first frame update
    void OnEnable() {
        EventManager.StartListening(EventManager.Event.INVENTORY_CHANGED, Init);
    }

    void OnDisable() {
        EventManager.StopListening(EventManager.Event.INVENTORY_CHANGED, Init);
        referencedCollection.onNewItemAdded -= ShowItemHint;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.I) && !DialogueManager.instance.inDialogue) {
            if (!panel.activeInHierarchy) {
                Display();
                pageRecyclers.Push(Hide);
            }
        } else if (Input.GetKeyDown(KeyCode.Escape) && pageRecyclers.Count > 0) {
            pageRecyclers.Pop().Invoke();
        }
    }

    // Function only need to be registered when referencedCollection panel is active.
    void UpdateUI() {
        for (int i = 0; i < slots.Length; i++) {
            if (i < referencedCollection.Size()) {
                slots[i].SetItem(referencedCollection.items[i]);
            } else {
                slots[i].ClearSlot();
            }
        }
    }

    void Display() {
        referencedCollection.onItemChanged += UpdateUI;
        Inventory.instance.onItemInspected += ZoomToShowItem;
        // The referencedCollection is closed every time a special item is used.
        UpdateUI();
        panel.SetActive(true);

        // Freeze the movement of the player
        UiStatus.OpenUI();

        //Set the first selected object to be the first slot
        if (referencedCollection.Size() != 0) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSlot);
        }
    }

    void Hide() {
        Debug.Log("Closing inventory UI");
        referencedCollection.onItemChanged -= UpdateUI;
        Inventory.instance.onItemInspected -= ZoomToShowItem;
        panel.SetActive(false);
        // Restore the movement of the player
        UiStatus.CloseUI();
    }

    public void ZoomToShowItem(Item item) {
        pageRecyclers.Push(zoomBox.Hide);
        zoomBox.Show(item);
    }

    public void ShowItemHint(Item item) {
        itemHint.Show(item);
    }
}
