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
    }
    
    public void StartChoice(params Choice[] choices) {
        isActive = true;
        this.choices = choices;
    }

    public void SetChoice(int i) {
        UiStatus.CloseUI();
        choiceIndex = i;

        choices[i].InvokeEvent();

        choiceIndex = -1;
        choiceHolder.SetActive(false);
        choiceButtons[2].SetActive(false);
        choiceButtons[3].SetActive(false);
        InputManager.choiceButtonActivated = false;
    }

    private void ActivateChoices() {
        UiStatus.OpenUI();
        InputManager.choiceButtonActivated = true;
        choiceHolder.SetActive(true);

        for (int i = 0; i < choices.Length; i++) {
            GameObject choiceButton = choiceButtons[i];
            choiceButton.SetActive(true);
            choiceButton.GetComponentInChildren<Text>().text = choices[i].choiceText;
            choiceButton.GetComponentInChildren<Button>().interactable = choices[i].GetEnabled();
        }       
    }
}
