using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hacker : Interactable
{
    public Conversation convo1, convo2, convo3, convo4;
    public Choice choice1, choice2, choice3;
    public bool choice3Enabled = false;
    
    void Update() {
        TryInteract();
    }

    public override void Interact()
    {
        DialogueManager.instance.StartConversation(convo1);

        choice1.SetEvent(Choice1);
        choice2.SetEvent(Choice2);
        choice3.SetEvent(Choice3);

        choice1.Enable(true);
        choice2.Enable(true);
        choice3.Enable(choice3Enabled);

        ChoiceManager.instance.StartChoice(choice1, choice2, choice3);
    }

    public void Choice1(object o = null) {
        DialogueManager.instance.StartConversation(convo2);
    }
    public void Choice2(object o = null) {
        DialogueManager.instance.StartConversation(convo3);
        choice3Enabled = true;
    }
    public void Choice3(object o = null) {
        DialogueManager.instance.StartConversation(convo4);
    }
}
