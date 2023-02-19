using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hacker : Interactable
{
    public Conversation convo1, convo2, convo3, convo4;
    public string choiceText1, choiceText2, choiceText3;
    private Choice choice1, choice2, choice3;
    private bool choice3Activated = false;

    void Awake() {
        InitialiseChoice();
    }

    void Update() {
        TryInteract();
    }

    public override void Interact()
    {
        DialogueManager.instance.StartConversation(convo1);

        choice3.activated = choice3Activated;

        ChoiceManager.instance.StartChoice(choice1, choice2, choice3);
    }

    private void InitialiseChoice() {
        choice1 = new Choice(choiceText1, Choice1);
        choice2 = new Choice(choiceText2, Choice2);
        choice3 = new Choice(choiceText3, choice3Activated, Choice3);
    }

    public void Choice1(object o = null) {
        DialogueManager.instance.StartConversation(convo2);
    }

    public void Choice2(object o = null) {
        DialogueManager.instance.StartConversation(convo3);
        choice3Activated = true;
    }

    public void Choice3(object o = null) {
        DialogueManager.instance.StartConversation(convo4);
    }
}
