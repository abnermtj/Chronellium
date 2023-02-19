using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager instance;

    private void Awake() {
        if (ChoiceManager.instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private Choice[] choices;
    public GameObject[] choiceButtons;
    public GameObject choiceHolder;
    public bool inChoice;

    private int choiceIndex = -1;
    private bool isActive;

    // Update is called once per frame
    void Update() {
        if (isActive && !DialogueManager.instance.inDialogue) {
            Debug.Assert(2 <= choices.Length && choices.Length <= 4);
            isActive = false;

            ActivateChoices();

            // When the start choice, set the first button to be selected 
            // so that the player can navigate using keyboard
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(choiceButtons[0]);
        }

        // Must rely on InputManager to receive the signal of the 
        // player pressing the submit button to set a choice.
        // See documentation in InputManager for further explanation.
        if (InputManager.choiceButtonActivated && choiceIndex != -1) {
            InputManager.choiceButtonActivated = false;
            choices[choiceIndex].choiceEvent.Invoke(null);

            choiceIndex = -1;
            choiceHolder.SetActive(false);
            for (int k = 0; k < choiceButtons.Length; k++) {
                choiceButtons[0].SetActive(false);
            }
            inChoice = false;       
        }
    }
    
    public void StartChoice(params Choice[] choices) {
        isActive = true;
        this.choices = choices;
    }

    public void SetChoice(int i) {
        UiStatus.CloseUI();
        choiceIndex = i;
    }

    private void ActivateChoices() {
        UiStatus.OpenUI();
        inChoice = true;
        choiceHolder.SetActive(true);

        for (int i = 0; i < choices.Length; i++) {
            GameObject choiceButton = choiceButtons[i];
            choiceButton.SetActive(true);
            choiceButton.GetComponentInChildren<Text>().text = choices[i].choiceText;
            choiceButton.GetComponentInChildren<Button>().interactable = choices[i].activated;
        }       
    }
}
