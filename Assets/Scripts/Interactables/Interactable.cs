using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    // true when the player triggers the collider of the interactable object 
    // false when the player exits
    public bool playerInRange;

    void Update() {
        TryInteract();
    }

    public virtual void TryInteract()
    {
        if (UiStatus.isOpen) {
            // when the ui is open, eg inventory is open or in dialogue,
            // the player cannot interact with interactable objects 
            return;
        }

        // when the player is in the range of the interactable object and
        // at the same time the player press "Z", Interact() is called
        if (InputManager.interactButtonActivated && playerInRange) {
            InputManager.interactButtonActivated = false;
            Interact();
        }
    }

    public abstract void Interact();
 
    private void OnTriggerEnter(Collider collision)
    {
        if (IsPlayer(collision.gameObject)) {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (IsPlayer(collision.gameObject)) {
            playerInRange = false;
        }
    }
    private bool IsPlayer(GameObject otherObject) {
        return otherObject.CompareTag("Player");
    }
}
