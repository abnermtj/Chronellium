using UnityEngine;

public class InputManager : MonoBehaviour
{
    // The submitButton(currently 'J') can be detected by multiple scripts
    // such as Item.Use() and Interactable.TryInteract(). To avoid the 
    // submit button being detected by more than one script, create a boolean 
    //  submitButtonDetect in GamaManager that is set to true every time
    // the submit button is pressed, and set to false when one of the sript
    // detects it.
    // Currently, the submit button is used by Item.Use(), DialogueManager.Update() 
    // and Interactable.TryInteract() and ChoiceManager.SetChoice().
    public static bool itemUseButtonActivated;
    public static bool interactButtonActivated;
    public static bool dialogButtonActivated;
    public static bool choiceButtonActivated;

    void Awake() {
        // Need to reset static variables manually
        // otherwise they might not be reset when
        // the scene restarts
        itemUseButtonActivated = false;
        interactButtonActivated = false;
        dialogButtonActivated = false;
        choiceButtonActivated = false;
    }

     // Priority: itemUseButtonActivated > dialogButtonActivated = choiceButtonActivated > interactButtonActivated
    private void Update()
    {
        if (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0))
        {
            Debug.Log("interactive button:" + interactButtonActivated);
            Debug.Log("choice button:" + choiceButtonActivated);
            Debug.Log("dialog button:" + dialogButtonActivated);

            if (!itemUseButtonActivated && !choiceButtonActivated && DialogueManager.instance.inDialogue)
            {
                dialogButtonActivated = true;
            } 

            if (!itemUseButtonActivated && !dialogButtonActivated && !choiceButtonActivated)
            {
                interactButtonActivated = true;
            }
        }
    }
}