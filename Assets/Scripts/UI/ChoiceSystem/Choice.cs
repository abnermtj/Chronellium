using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class Choice 
{
    public string choiceText;
    public bool activated;
    public UnityAction<object> choiceEvent;

     public Choice(string choiceText, UnityAction<object> choiceEvent) {
        this.choiceText = choiceText;
        this.activated = true;
        this.choiceEvent = choiceEvent;
    }

    public Choice(string choiceText, bool activated, UnityAction<object> choiceEvent) {
        this.choiceText = choiceText;
        this.activated = activated;
        this.choiceEvent = choiceEvent;
    }
}
