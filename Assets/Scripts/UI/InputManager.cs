using UnityEngine;

public class InputManager : MonoBehaviour
{
    // When the submit button is pressed, the signal can 
    // be detected by multiple scripts which cause undesired
    // behavirous. e.g., when the submit button is pressed in 
    // a choice, the signal can be captured by both the DialogueManager
    // and the choice button, causing one dialogue line skipped 
    // as the choice is made.
    // Thus, all signal of the player pressing the submit button
    // must be caught by this script first, which ensures that only
    // one button is activated each time the submit button is pressed.

    // To change the submit button, go to Project Settings -> Input -> Submit
    public static bool interactButtonActivated;
    public static bool dialogButtonActivated;
    public static bool choiceButtonActivated;

    void Awake() {
        // Need to reset static variables manually
        // otherwise they might not be reset when
        // the scene restarts.
        interactButtonActivated = false;
        dialogButtonActivated = false;
        choiceButtonActivated = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Submit") || Input.GetMouseButtonDown(0))
        {
            if (ChoiceManager.instance.inChoice) {
                choiceButtonActivated = true;
                return;
            }
            
            if (DialogueManager.instance.inDialogue)
            {
                dialogButtonActivated = true;
                return;
            } 

            interactButtonActivated = true;
        }
    }
}