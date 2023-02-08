using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackGameManager : MonoBehaviour
{
    public static HackGameManager instance;
    public InputNodeView[] allInputNodes;
    // public OutputNodeView[] allOutputNodes;
    public Camera gameCamera;

    void Awake() {
        if (instance == null) instance = this;
    }

    public void Kickoff() {
        foreach (InputNodeView inputNodeView in allInputNodes) {
            inputNodeView.StartStreamWithInput();
        }
    }
}
