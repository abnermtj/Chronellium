using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nara1st : Interactable
{
    [SerializeField] private bool hasSpoken = false;
    public Conversation abrasiveStart;

    public override void Interact() {
        if (!hasSpoken) {
            DialogueManager.instance.StartConversation(abrasiveStart);
            hasSpoken = true;
        }
    }
}
