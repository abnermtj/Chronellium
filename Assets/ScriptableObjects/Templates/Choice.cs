using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

[CreateAssetMenu]
public class Choice : ScriptableObject
{
    public string choiceText;
    private bool isEnabled;
    private UnityAction<object> choiceEvent;

    public void Enable(bool isEnabled) {
        this.isEnabled = isEnabled;
    }

    public bool GetEnabled() {
        return isEnabled;
    }

    public void SetEvent(UnityAction<object> choiceEvent) {
        this.choiceEvent = choiceEvent;
    }

    public void InvokeEvent(object inputParam = null) {
        choiceEvent.Invoke(inputParam);
    }
}
